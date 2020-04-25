using ENode.Domain;
using _Eventual2PCEvents = Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 事务参与方预提交失败领域异常
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    /// <typeparam name="TTransactionPreparation">事务准备</typeparam>
    public interface ITransactionParticipantPreCommitDomainException<TParticipant, TTransactionPreparation>
        : _Eventual2PCEvents.ITransactionParticipantPreCommitFailed<TParticipant, TTransactionPreparation>
        , IDomainException
        where TParticipant : class, ITransactionParticipant
        where TTransactionPreparation : class, ITransactionPreparation
    {
    }

    /// <summary>
    /// 事务参与方预提交失败领域异常
    /// </summary>
    /// <typeparam name="TParticipant">事务参与方</typeparam>
    public interface ITransactionParticipantPreCommitDomainException<TParticipant>
        : _Eventual2PCEvents.ITransactionParticipantPreCommitFailed<TParticipant>
        , IDomainException
        where TParticipant : class, ITransactionParticipant
    {      
    }
}
