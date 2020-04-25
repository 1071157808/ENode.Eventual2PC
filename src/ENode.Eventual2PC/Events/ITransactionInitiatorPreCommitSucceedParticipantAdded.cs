﻿using ENode.Eventing;
using _Eventual2PCEvents = Eventual2PC.Events;

namespace ENode.Eventual2PC.Events
{
    /// <summary>
    /// 预提交成功的事务参与方已添加事件接口
    /// </summary>
    /// <typeparam name="TInitiator">事务发起方</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID</typeparam>
    public interface ITransactionInitiatorPreCommitSucceedParticipantAdded<TInitiator, TAggregateRootId>
        : _Eventual2PCEvents.ITransactionInitiatorPreCommitSucceedParticipantAdded<TInitiator>
        , IDomainEvent<TAggregateRootId>
        where TInitiator : class, ITransactionInitiator
    {
     
    }
}
