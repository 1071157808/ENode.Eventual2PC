using ENode.Eventing;
using _Eventual2PCEvents = Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方已提交事件
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    /// <typeparam name="TTransactionPreparation">事务准备</typeparam>
    public interface ITransactionParticipantCommitted<TParticipant, TAggregateRootId, TTransactionPreparation>
        : _Eventual2PCEvents.ITransactionParticipantCommitted<TParticipant, TTransactionPreparation>
        , IDomainEvent<TAggregateRootId>
        where TParticipant : class, ITransactionParticipant
        where TTransactionPreparation : class, global::Eventual2PC.ITransactionPreparation
    {
    }
}
