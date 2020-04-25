using ENode.Domain;
using _Eventual2PC = Eventual2PC;

namespace ENode.Eventual2PC
{
    /// <summary>
    /// 事务参与方
    /// </summary>
    public interface ITransactionParticipant : _Eventual2PC.ITransactionParticipant, IAggregateRoot
    {
    }
}
