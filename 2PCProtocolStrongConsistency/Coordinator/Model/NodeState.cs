using Coordinator.Enums;

namespace Coordinator.Model
{
    public record NodeState(Guid TransactionId)
    {
        public Guid Id { get; set; }
        public Node Node { get; set; }

        // 1. Hazırlık Aşaması Durumu
        public ReadyType IsReady { get; set; }
        // 2. Taahüt Aşaması Durumu
        public TransactionState TransactionState { get; set; }
    }
}
