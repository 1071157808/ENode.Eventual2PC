using ENode.Eventing;
using _Eventual2PCEvents = Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方已回滚事件
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    /// <typeparam name="TTransactionPreparation">事务准备</typeparam>
    public interface ITransactionParticipantRolledback<TParticipant, TAggregateRootId, TTransactionPreparation>
        : _Eventual2PCEvents.ITransactionParticipantRolledback<TParticipant, TTransactionPreparation>
        , IDomainEvent<TAggregateRootId>
        where TParticipant : class, ITransactionParticipant
        where TTransactionPreparation : class, ITransactionPreparation
    {
    }
}
