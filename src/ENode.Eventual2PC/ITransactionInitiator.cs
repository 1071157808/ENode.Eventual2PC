using ENode.Domain;
using _Eventual2PC = Eventual2PC;

namespace ENode.Eventual2PC
{
    /// <summary>
    /// 事务发起方
    /// </summary>
    public interface ITransactionInitiator : _Eventual2PC.ITransactionInitiator, IAggregateRoot
    {
    }
}
