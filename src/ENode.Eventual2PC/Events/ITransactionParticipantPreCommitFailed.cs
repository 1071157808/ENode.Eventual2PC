using ENode.Eventing;
using _Eventual2PCEvents = Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方预提交失败事件（或领域异常）
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    /// <typeparam name="TTransactionPreparation">事务准备</typeparam>
    public interface ITransactionParticipantPreCommitFailed<TParticipant, TAggregateRootId, TTransactionPreparation>
        : _Eventual2PCEvents.ITransactionParticipantPreCommitFailed<TParticipant, TTransactionPreparation>
        , IDomainEvent<TAggregateRootId>
        where TParticipant : class, ITransactionParticipant
        where TTransactionPreparation : class, ITransactionPreparation
    {
    }

    /// <summary>
    /// 事务参与方预提交失败事件（或领域异常）
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    public interface ITransactionParticipantPreCommitFailed<TParticipant, TAggregateRootId>
        : _Eventual2PCEvents.ITransactionParticipantPreCommitFailed<TParticipant>
        , IDomainEvent<TAggregateRootId>
        where TParticipant : class, ITransactionParticipant
    {
        
    }
}
