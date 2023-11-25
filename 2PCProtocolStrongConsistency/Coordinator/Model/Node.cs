namespace Coordinator.Model
{
    public record Node(string Name)
    {
        public Guid Id { get; set; }

        public ICollection<NodeState> NodeState { get; set; }
    }
}
