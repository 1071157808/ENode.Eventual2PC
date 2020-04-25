using ENode.Eventing;
using _Eventual2PCEvents = Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务已完成事件接口
    /// </summary>
    /// <typeparam name="TInitiator">事务发起方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    public interface ITransactionInitiatorTransactionCompleted<TInitiator, TAggregateRootId>
        : _Eventual2PCEvents.ITransactionInitiatorTransactionCompleted<TInitiator>
        , IDomainEvent<TAggregateRootId>
        where TInitiator : class, ITransactionInitiator
    {
        
    }
}
