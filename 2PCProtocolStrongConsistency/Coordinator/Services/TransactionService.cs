using Coordinator.Enums;
using Coordinator.Model;
using Coordinator.Model.Context;
using Coordinator.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services
{
    public class TransactionService : ITransactionService
    {
        readonly TwoPhaseCommitContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        HttpClient _orderHttpClient;
        HttpClient _stockHttpClient;
        HttpClient _paymentHttpClient;

        public TransactionService(TwoPhaseCommitContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;

            _paymentHttpClient = _httpClientFactory.CreateClient("PaymentAPI");
            _stockHttpClient = _httpClientFactory.CreateClient("StockAPI");
            _orderHttpClient = _httpClientFactory.CreateClient("OrderAPI");
        }

        public async Task<Guid> CreateTransactionAsync()
        {
            Guid transactionId = Guid.NewGuid();

            var nodes = await _context.Nodes.ToListAsync();

            nodes.ForEach(node =>
            {
                node.NodeState = new List<NodeState>()
                {
                    new(transactionId)
                    {
                        IsReady = Enums.ReadyType.Pending,
                        TransactionState = Enums.TransactionState.Pending
                    }
                };
            });

            await _context.SaveChangesAsync();

            return transactionId;
        }
        public async Task PrepareServicesAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
               .Include(s => s.Node)
               .Where(s => s.TransactionId == transactionId)
               .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("ready"),
                        "Stock.API" => _stockHttpClient.GetAsync("ready"),
                        "Payment.API" => _paymentHttpClient.GetAsync("ready"),
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());

                    transactionNode.IsReady = result ? Enums.ReadyType.Ready : Enums.ReadyType.Unready;
                }
                catch (Exception ex)
                {
                    transactionNode.IsReady = Enums.ReadyType.Unready;
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckReadyServicesAsync(Guid transactionId)
        {
            return (await _context.NodeStates
                .Include(s => s.Node)
                .Where(s => s.TransactionId == transactionId)
                .ToListAsync()).TrueForAll(ns => ns.IsReady == Enums.ReadyType.Ready);
        }

        public async Task CommitAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                .Include(s => s.Node)
                .Where(s => s.TransactionId == transactionId)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("commit"),
                        "Stock.API" => _stockHttpClient.GetAsync("commit"),
                        "Payment.API" => _paymentHttpClient.GetAsync("commit"),
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());

                    transactionNode.TransactionState = result ? TransactionState.Done : TransactionState.Abort;
                }
                catch (Exception ex)
                {
                    transactionNode.TransactionState = TransactionState.Abort;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
        {
            return (await _context.NodeStates
                          .Include(s => s.Node)
                          .Where(s => s.TransactionId == transactionId)
                          .ToListAsync())
                          .TrueForAll(ns => ns.TransactionState == Enums.TransactionState.Done);
        }

        public async Task RollBackAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                                  .Include(s => s.Node)
                                  .Where(s => s.TransactionId == transactionId)
                                  .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    if(transactionNode.TransactionState == Enums.TransactionState.Done)
                    {
                        _ = await (transactionNode.Node.Name switch
                        {
                            "Order.API" => _orderHttpClient.GetAsync("rollback"),
                            "Stock.API" => _stockHttpClient.GetAsync("rollback"),
                            "Payment.API" => _paymentHttpClient.GetAsync("rollback"),
                        });
                    }

                    transactionNode.TransactionState = TransactionState.Abort;
                }
                catch (Exception ex)
                {
                    transactionNode.TransactionState = TransactionState.Abort;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
