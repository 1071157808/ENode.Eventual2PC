using ENode.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventual2PC
{
    /// <summary>
    /// 事务发起方
    /// </summary>
    /// <typeparam name="TTransactionInitiator">事务发起方实现类</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID类型</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorBase<TTransactionInitiator, TAggregateRootId>
        : AggregateRoot<TAggregateRootId>, ITransactionInitiator
        where TTransactionInitiator : TransactionInitiatorBase<TTransactionInitiator, TAggregateRootId>
    {
        private string _transactionId;
        private byte _transactionType;
        private List<global::Eventual2PC.TransactionParticipantInfo> _allTransactionParticipants;
        private List<global::Eventual2PC.TransactionParticipantInfo> _preCommitSuccessTransactionParticipants;
        private List<global::Eventual2PC.TransactionParticipantInfo> _preCommitFailTransactionParticipants;
        private List<global::Eventual2PC.TransactionParticipantInfo> _committedTransactionParticipants;
        private List<global::Eventual2PC.TransactionParticipantInfo> _rolledbackTransactionParticipants;

        /// <summary>
        /// 事务发起方
        /// </summary>
        /// <returns></returns>
        protected TransactionInitiatorBase() : base() { }

        /// <summary>
        /// 事务发起方
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected TransactionInitiatorBase(TAggregateRootId id) : base(id) { }

        /// <summary>
        /// 事务发起方
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        protected TransactionInitiatorBase(TAggregateRootId id, int version) : base(id, version) { }

        /// <summary>
        /// 是否事务处理中
        /// </summary>
        /// <returns></returns>
        public bool IsTransactionProcessing
        {
            get
            {
                return _transactionType != 0;
            }
        }

        /// <summary>
        /// 是否所有预提交参与者都已添加且都成功
        /// </summary>
        /// <returns></returns>
        protected bool IsAllPreCommitParticipantAddedAndSuccess()
        {
            return _allTransactionParticipants.Count == _preCommitSuccessTransactionParticipants.Count;
        }

        /// <summary>
        /// 是否所有预提交参与者都已添加
        /// </summary>
        /// <returns></returns>
        protected bool IsAllPreCommitParticipantAdded()
        {
            return _allTransactionParticipants.Count == _preCommitSuccessTransactionParticipants.Count + _preCommitFailTransactionParticipants.Count;
        }

        /// <summary>
        /// 是否所有已提交和已回滚的参与者都已添加
        /// </summary>
        /// <returns></returns>
        protected bool IsAllCommittedAndRolledbackParticipantAdded()
        {
            return _allTransactionParticipants.Count == _committedTransactionParticipants.Count + _rolledbackTransactionParticipants.Count;
        }

        /// <summary>
        /// 添加预提交成功的参与者
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="participantInfo"></param>
        public void AddPreCommitSuccessParticipant(string transactionId, byte transactionType, global::Eventual2PC.TransactionParticipantInfo participantInfo)
        {
            if (participantInfo == null)
            {
                throw new ArgumentNullException(nameof(participantInfo));
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (transactionType == 0)
            {
                throw new ArgumentNullException(nameof(transactionType));
            }
            if (_transactionType == 0)
            {
                throw new ApplicationException($"Not in transaction, couldn't AddPreCommitSuccessParticipant.");
            }
            if (transactionType != _transactionType)
            {
                throw new ApplicationException($"Transaction type {transactionType} is not same as {_transactionType}");
            }
            if (!string.IsNullOrEmpty(_transactionId) && transactionId != _transactionId)
            {
                throw new ApplicationException($"Transaction id {transactionId} is not same as {_transactionId}");
            }
            if (_preCommitSuccessTransactionParticipants.Count > 0)
            {
                participantInfo.ValidateParticipantMustNotExists(_preCommitSuccessTransactionParticipants);
            }
            if (_preCommitFailTransactionParticipants.Count > 0)
            {
                participantInfo.ValidateParticipantMustNotExists(_preCommitFailTransactionParticipants);
            }

            ApplyEvent(CreatePreCommitSuccessParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllPreCommitParticipantAddedAndSuccess())
            {
                // 所有参与者的预提交都已成功处理
                ApplyEvent(CreateAllParticipantPreCommitSucceedEvent(transactionId, transactionType, _preCommitSuccessTransactionParticipants));
            }
        }

        /// <summary>
        /// 添加预提交失败的参与者
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="participantInfo"></param>
        public void AddPreCommitFailedParticipant(string transactionId, byte transactionType, global::Eventual2PC.TransactionParticipantInfo participantInfo)
        {
            if (participantInfo == null)
            {
                throw new ArgumentNullException(nameof(participantInfo));
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (transactionType == 0)
            {
                throw new ArgumentNullException(nameof(transactionType));
            }
            if (_transactionType == 0)
            {
                throw new ApplicationException($"Not in transaction, couldn't AddPreCommitSuccessParticipant.");
            }
            if (transactionType != _transactionType)
            {
                throw new ApplicationException($"Transaction type {transactionType} is not same as {_transactionType}");
            }
            if (!string.IsNullOrEmpty(_transactionId) && transactionId != _transactionId)
            {
                throw new ApplicationException($"Transaction id {transactionId} is not same as {_transactionId}");
            }
            if (_preCommitSuccessTransactionParticipants.Count > 0)
            {
                participantInfo.ValidateParticipantMustNotExists(_preCommitSuccessTransactionParticipants);
            }
            if (_preCommitFailTransactionParticipants.Count > 0)
            {
                participantInfo.ValidateParticipantMustNotExists(_preCommitFailTransactionParticipants);
            }

            ApplyEvent(CreatePreCommitFailParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllPreCommitParticipantAdded())
            {
                // 所有预提交已添加
                ApplyEvent(CreateAnyParticipantPreCommitFailedEvent(transactionId, transactionType, _preCommitSuccessTransactionParticipants));
            }
        }

        /// <summary>
        /// 添加已提交的参与者
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="participantInfo"></param>
        public void AddCommittedParticipant(string transactionId, byte transactionType, global::Eventual2PC.TransactionParticipantInfo participantInfo)
        {
            if (participantInfo == null)
            {
                throw new ArgumentNullException(nameof(participantInfo));
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (transactionType == 0)
            {
                throw new ArgumentNullException(nameof(transactionType));
            }
            if (_transactionType == 0)
            {
                throw new ApplicationException($"Not in transaction, couldn't AddRolledbackParticipant.");
            }
            if (transactionType != _transactionType)
            {
                throw new ApplicationException($"Transaction type {transactionType} is not same as {_transactionType}");
            }
            if (!string.IsNullOrEmpty(_transactionId) && transactionId != _transactionId)
            {
                throw new ApplicationException($"Transaction id {transactionId} is not same as {_transactionId}");
            }
            if (_committedTransactionParticipants.Count > 0)
            {
                participantInfo.ValidateParticipantMustNotExists(_preCommitSuccessTransactionParticipants);
            }
            if (_rolledbackTransactionParticipants.Count > 0)
            {
                participantInfo.ValidateParticipantMustNotExists(_preCommitFailTransactionParticipants);
            }

            ApplyEvent(CreateCommittedParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllCommittedAndRolledbackParticipantAdded())
            {
                // 所有参与者的提交和回滚都已处理
                ApplyEvent(CreateTransactionCompletedEvent(transactionId, transactionType, _rolledbackTransactionParticipants.Count == 0));
            }
        }

        /// <summary>
        /// 添加已回滚的参与者
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="participantInfo"></param>
        public void AddRolledbackParticipant(string transactionId, byte transactionType, global::Eventual2PC.TransactionParticipantInfo participantInfo)
        {
            if (participantInfo == null)
            {
                throw new ArgumentNullException(nameof(participantInfo));
            }
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (transactionType == 0)
            {
                throw new ArgumentNullException(nameof(transactionType));
            }
            if (_transactionType == 0)
            {
                throw new ApplicationException($"Not in transaction, couldn't AddRolledbackParticipant.");
            }
            if (transactionType != _transactionType)
            {
                throw new ApplicationException($"Transaction type {transactionType} is not same as {_transactionType}");
            }
            if (!string.IsNullOrEmpty(_transactionId) && transactionId != _transactionId)
            {
                throw new ApplicationException($"Transaction id {transactionId} is not same as {_transactionId}");
            }
            if (_committedTransactionParticipants.Count > 0)
            {
                participantInfo.ValidateParticipantMustNotExists(_preCommitSuccessTransactionParticipants);
            }
            if (_rolledbackTransactionParticipants.Count > 0)
            {
                participantInfo.ValidateParticipantMustNotExists(_preCommitFailTransactionParticipants);
            }

            ApplyEvent(CreateRolledbackParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllCommittedAndRolledbackParticipantAdded())
            {
                // 所有参与者的提交和回滚都已处理
                ApplyEvent(CreateTransactionCompletedEvent(transactionId, transactionType, _rolledbackTransactionParticipants.Count == 0));
            }
        }

        /// <summary>
        /// Create PreCommitSuccessParticipantAdded event
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="participantInfo"></param>
        protected abstract Events.ITransactionInitiatorPreCommitSucceedParticipantAdded<TTransactionInitiator, TAggregateRootId> CreatePreCommitSuccessParticipantAddedEvent(string transactionId, byte transactionType, global::Eventual2PC.TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create PreCommitFailParticipantAdded event
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="participantInfo"></param>
        protected abstract Events.ITransactionInitiatorPreCommitFailedParticipantAdded<TTransactionInitiator, TAggregateRootId> CreatePreCommitFailParticipantAddedEvent(string transactionId, byte transactionType, global::Eventual2PC.TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create AllParticipantPreCommitSucceed event
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="_preCommitSuccessTransactionParticipants"></param>
        protected abstract Events.ITransactionInitiatorAllParticipantPreCommitSucceed<TTransactionInitiator, TAggregateRootId> CreateAllParticipantPreCommitSucceedEvent(string transactionId, byte transactionType, List<global::Eventual2PC.TransactionParticipantInfo> _preCommitSuccessTransactionParticipants);


        /// <summary>
        /// Create AnyParticipantPreCommitFailed event
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="preCommitSuccessTransactionParticipants"></param>
        protected abstract Events.ITransactionInitiatorAnyParticipantPreCommitFailed<TTransactionInitiator, TAggregateRootId> CreateAnyParticipantPreCommitFailedEvent(string transactionId, byte transactionType, List<global::Eventual2PC.TransactionParticipantInfo> preCommitSuccessTransactionParticipants);

        /// <summary>
        /// Create CommittedParticipantAdded event
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="participantInfo"></param>
        protected abstract Events.ITransactionInitiatorCommittedParticipantAdded<TTransactionInitiator, TAggregateRootId> CreateCommittedParticipantAddedEvent(string transactionId, byte transactionType, global::Eventual2PC.TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create RolledbackParticipantAdded event
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="participantInfo"></param>
        protected abstract Events.ITransactionInitiatorRolledbackParticipantAdded<TTransactionInitiator, TAggregateRootId> CreateRolledbackParticipantAddedEvent(string transactionId, byte transactionType, global::Eventual2PC.TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create TransactionCompleted event
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="transactionType"></param>
        /// <param name="isCommitSuccess"></param>
        protected abstract Events.ITransactionInitiatorTransactionCompleted<TTransactionInitiator, TAggregateRootId> CreateTransactionCompletedEvent(string transactionId, byte transactionType, bool isCommitSuccess);
        

        /// <summary>
        /// Handle TransactionStarted event
        /// </summary>
        /// <param name="transactionType"></param>
        /// <param name="allTransactionParticipants"></param>
        protected void HandleTransactionStartedEvent(byte transactionType, IEnumerable<global::Eventual2PC.TransactionParticipantInfo> allTransactionParticipants)
        {
            _transactionType = transactionType;
            _allTransactionParticipants = new List<global::Eventual2PC.TransactionParticipantInfo>();
            if (allTransactionParticipants != null && allTransactionParticipants.Any())
            {
                _allTransactionParticipants.AddRange(allTransactionParticipants);
            }
            _preCommitSuccessTransactionParticipants = new List<global::Eventual2PC.TransactionParticipantInfo>();
            _preCommitFailTransactionParticipants = new List<global::Eventual2PC.TransactionParticipantInfo>();
            _committedTransactionParticipants = new List<global::Eventual2PC.TransactionParticipantInfo>();
            _rolledbackTransactionParticipants = new List<global::Eventual2PC.TransactionParticipantInfo>();
        }

        /// <summary>
        /// Handle PreCommitSuccessParticipantAdded event
        /// </summary>
        /// <param name="transactionId"></param>
        protected void HandlePreCommitSuccessParticipantAddedEvent(string transactionId)
        {
            _transactionId = transactionId;
        }

        /// <summary>
        /// Handle PreCommitFailParticipantAdded event
        /// </summary>
        /// <param name="transactionId"></param>
        protected void HandlePreCommitFailParticipantAddedEvent(string transactionId)
        {
            _transactionId = transactionId;
        }

        /// <summary>
        /// Handle TransactionCompleted event
        /// </summary>
        protected void HandleTransactionCompletedEvent()
        {
            _transactionType = 0;
            _transactionId = string.Empty;
            _allTransactionParticipants.Clear();
            _preCommitSuccessTransactionParticipants.Clear();
            _preCommitFailTransactionParticipants.Clear();
            _committedTransactionParticipants.Clear();
            _rolledbackTransactionParticipants.Clear();
        }
    }
}
