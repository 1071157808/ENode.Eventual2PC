using ENode.Domain;
using ENode.Eventual2PC.Events;
using Eventual2PC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventual2PC
{
    /// <summary>
    /// 事务发起方兼其他事务参与方
    /// </summary>
    /// <typeparam name="TTransactionInitiator">事务发起方实现类</typeparam>
    /// <typeparam name="TAggregateRootId">聚合根ID类型</typeparam>
    [Serializable]
    public abstract class TransactionInitiatorAlsoActAsOtherParticipantBase<TTransactionInitiator, TAggregateRootId>
        : AggregateRoot<TAggregateRootId>, ITransactionInitiator
        , ITransactionParticipant
        where TTransactionInitiator : TransactionInitiatorAlsoActAsOtherParticipantBase<TTransactionInitiator, TAggregateRootId>
    {
        private string _transactionId;
        private byte _transactionType;
        private List<TransactionParticipantInfo> _allTransactionParticipants;
        private List<TransactionParticipantInfo> _preCommitSuccessTransactionParticipants;
        private List<TransactionParticipantInfo> _preCommitFailTransactionParticipants;
        private List<TransactionParticipantInfo> _committedTransactionParticipants;
        private List<TransactionParticipantInfo> _rolledbackTransactionParticipants;
        private IDictionary<string, ITransactionPreparation> _transactionPreparations;

        /// <summary>
        /// 事务发起方兼其他事务参与方
        /// </summary>
        /// <returns></returns>
        protected TransactionInitiatorAlsoActAsOtherParticipantBase() : base() { }

        /// <summary>
        /// 事务发起方兼其他事务参与方
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected TransactionInitiatorAlsoActAsOtherParticipantBase(TAggregateRootId id) : base(id) { }

        /// <summary>
        /// 事务发起方兼其他事务参与方
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        protected TransactionInitiatorAlsoActAsOtherParticipantBase(TAggregateRootId id, int version) : base(id, version) { }

        #region Initiator

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
            return _allTransactionParticipants.Count == _committedTransactionParticipants.Count
                || _preCommitSuccessTransactionParticipants.Count == _rolledbackTransactionParticipants.Count;
        }

        /// <summary>
        /// 添加预提交成功的参与者（依次发布 PreCommitSuccessParticipantAdded 、 AllParticipantPreCommitSucceed（或 AnyParticipantPreCommitFailed） 事件）
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        public void AddPreCommitSuccessParticipant(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
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
                throw new ApplicationException($"Initiator {Id} is not in transaction, couldn't execute AddPreCommitSuccessParticipant command.");
            }
            if (transactionType != _transactionType)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction type {transactionType} is not same as {_transactionType}");
            }
            if (!string.IsNullOrEmpty(_transactionId) && transactionId != _transactionId)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction id {transactionId} is not same as {_transactionId}");
            }
            if (_preCommitSuccessTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_preCommitSuccessTransactionParticipants))
            {
                return;
            }
            if (_preCommitFailTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_preCommitFailTransactionParticipants))
            {
                return;
            }
            if (_allTransactionParticipants == null || !_allTransactionParticipants.Any(w => w.ParticipantId == participantInfo.ParticipantId))
            {
                return;
            }

            ApplyEvent(CreatePreCommitSuccessParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllPreCommitParticipantAddedAndSuccess())
            {
                // 所有参与者的预提交都已成功处理
                ApplyEvent(CreateAllParticipantPreCommitSucceedEvent(transactionId, transactionType, _preCommitSuccessTransactionParticipants));
            }
            else if (IsAllPreCommitParticipantAdded())
            {
                // 所有预提交已添加
                ApplyEvent(CreateAnyParticipantPreCommitFailedEvent(transactionId, transactionType, _preCommitSuccessTransactionParticipants));
            }
        }

        /// <summary>
        /// 添加预提交失败的参与者（依次发布 PreCommitFailParticipantAdded 、 AnyParticipantPreCommitFailed 事件）
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        public void AddPreCommitFailedParticipant(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
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
                throw new ApplicationException($"Initiator {Id} is not in transaction, couldn't execute AddPreCommitFailedParticipant command.");
            }
            if (transactionType != _transactionType)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction type {transactionType} is not same as {_transactionType}");
            }
            if (!string.IsNullOrEmpty(_transactionId) && transactionId != _transactionId)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction id {transactionId} is not same as {_transactionId}");
            }
            if (_preCommitSuccessTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_preCommitSuccessTransactionParticipants))
            {
                return;
            }
            if (_preCommitFailTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_preCommitFailTransactionParticipants))
            {
                return;
            }
            if (_allTransactionParticipants == null || !_allTransactionParticipants.Any(w => w.ParticipantId == participantInfo.ParticipantId))
            {
                return;
            }

            ApplyEvent(CreatePreCommitFailParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllPreCommitParticipantAdded())
            {
                // 所有预提交已添加
                ApplyEvent(CreateAnyParticipantPreCommitFailedEvent(transactionId, transactionType, _preCommitSuccessTransactionParticipants));
            }
        }

        /// <summary>
        /// 添加已提交的参与者（依次发布 CommittedParticipantAdded 、 TransactionCompleted 事件）
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        public void AddCommittedParticipant(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
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
                throw new ApplicationException($"Initiator {Id} is not in transaction, couldn't execute AddCommittedParticipant command.");
            }
            if (transactionType != _transactionType)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction type {transactionType} is not same as {_transactionType}");
            }
            if (!string.IsNullOrEmpty(_transactionId) && transactionId != _transactionId)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction id {transactionId} is not same as {_transactionId}");
            }
            if (_committedTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_committedTransactionParticipants))
            {
                return;
            }
            if (_rolledbackTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_rolledbackTransactionParticipants))
            {
                return;
            }
            if (!IsAllPreCommitParticipantAdded())
            {
                throw new ApplicationException("Initiator {Id} didn't received all PreCommit participant, couldn't execute AddCommittedParticipant command.");
            }
            if (_allTransactionParticipants == null || !_allTransactionParticipants.Any(w => w.ParticipantId == participantInfo.ParticipantId))
            {
                return;
            }

            ApplyEvent(CreateCommittedParticipantAddedEvent(transactionId, transactionType, participantInfo));
            if (IsAllCommittedAndRolledbackParticipantAdded())
            {
                // 所有参与者的提交和回滚都已处理
                ApplyEvent(CreateTransactionCompletedEvent(transactionId, transactionType, _rolledbackTransactionParticipants.Count == 0));
            }
        }

        /// <summary>
        /// 添加已回滚的参与者（依次发布 RolledbackParticipantAdded 、 TransactionCompleted 事件）
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        public void AddRolledbackParticipant(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo)
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
                throw new ApplicationException($"Initiator {Id} is not in transaction, couldn't execute AddRolledbackParticipant command.");
            }
            if (transactionType != _transactionType)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction type {transactionType} is not same as {_transactionType}");
            }
            if (!string.IsNullOrEmpty(_transactionId) && transactionId != _transactionId)
            {
                throw new ApplicationException($"Initiator {Id}'s transaction id {transactionId} is not same as {_transactionId}");
            }
            if (_committedTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_committedTransactionParticipants))
            {
                return;
            }
            if (_rolledbackTransactionParticipants.Count > 0 && participantInfo.IsParticipantAlreadyExists(_rolledbackTransactionParticipants))
            {
                return;
            }
            if (!IsAllPreCommitParticipantAdded())
            {
                throw new ApplicationException("Initiator {Id} didn't received all PreCommit participant, couldn't execute AddRolledbackParticipant command.");
            }
            if (_allTransactionParticipants == null || !_allTransactionParticipants.Any(w => w.ParticipantId == participantInfo.ParticipantId))
            {
                return;
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
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        protected abstract TransactionInitiatorPreCommitSucceedParticipantAddedBase<TTransactionInitiator, TAggregateRootId> CreatePreCommitSuccessParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create PreCommitFailParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        protected abstract TransactionInitiatorPreCommitFailedParticipantAddedBase<TTransactionInitiator, TAggregateRootId> CreatePreCommitFailParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create AllParticipantPreCommitSucceed event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="preCommitSuccessTransactionParticipants"></param>
        protected abstract TransactionInitiatorAllParticipantPreCommitSucceedBase<TTransactionInitiator, TAggregateRootId> CreateAllParticipantPreCommitSucceedEvent(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSuccessTransactionParticipants);

        /// <summary>
        /// Create AnyParticipantPreCommitFailed event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="preCommitSuccessTransactionParticipants"></param>
        protected abstract TransactionInitiatorAnyParticipantPreCommitFailedBase<TTransactionInitiator, TAggregateRootId> CreateAnyParticipantPreCommitFailedEvent(string transactionId, byte transactionType, IEnumerable<TransactionParticipantInfo> preCommitSuccessTransactionParticipants);

        /// <summary>
        /// Create CommittedParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        protected abstract TransactionInitiatorCommittedParticipantAddedBase<TTransactionInitiator, TAggregateRootId> CreateCommittedParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create RolledbackParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="participantInfo"></param>
        protected abstract TransactionInitiatorRolledbackParticipantAddedBase<TTransactionInitiator, TAggregateRootId> CreateRolledbackParticipantAddedEvent(string transactionId, byte transactionType, TransactionParticipantInfo participantInfo);

        /// <summary>
        /// Create TransactionCompleted event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionType">事务类型</param>
        /// <param name="isCommitSuccess"></param>
        protected abstract TransactionInitiatorTransactionCompletedBase<TTransactionInitiator, TAggregateRootId> CreateTransactionCompletedEvent(string transactionId, byte transactionType, bool isCommitSuccess);

        /// <summary>
        /// Handle TransactionStarted event
        /// </summary>
        /// <param name="transactionType">事务类型</param>
        /// <param name="allTransactionParticipants"></param>
        protected void HandleTransactionStartedEvent(byte transactionType, IEnumerable<TransactionParticipantInfo> allTransactionParticipants)
        {
            _transactionType = transactionType;
            _allTransactionParticipants = new List<TransactionParticipantInfo>();
            if (allTransactionParticipants == null || !allTransactionParticipants.Any())
            {
                throw new ApplicationException($"Initiator {Id} hasn't any participant in transaction [{transactionType}].");
            }
            if (allTransactionParticipants.Any(w => w.ParticipantId == Id.ToString()))
            {
                throw new ApplicationException($"Initiator {Id} cann't act as participant in transaction [{transactionType}].");
            }
            _allTransactionParticipants.AddRange(allTransactionParticipants);
            _preCommitSuccessTransactionParticipants = new List<TransactionParticipantInfo>();
            _preCommitFailTransactionParticipants = new List<TransactionParticipantInfo>();
            _committedTransactionParticipants = new List<TransactionParticipantInfo>();
            _rolledbackTransactionParticipants = new List<TransactionParticipantInfo>();
        }

        /// <summary>
        /// Handle PreCommitSuccessParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected void HandlePreCommitSuccessParticipantAddedEvent(string transactionId, TransactionParticipantInfo transactionParticipant)
        {
            _transactionId = transactionId;
            _preCommitSuccessTransactionParticipants.Add(transactionParticipant);
        }

        /// <summary>
        /// Handle PreCommitFailParticipantAdded event
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected void HandlePreCommitFailParticipantAddedEvent(string transactionId, TransactionParticipantInfo transactionParticipant)
        {
            _transactionId = transactionId;
            _preCommitFailTransactionParticipants.Add(transactionParticipant);
        }

        /// <summary>
        /// Handle CommittedParticipantAdded event
        /// </summary>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected void HandleCommittedParticipantAddedEvent(TransactionParticipantInfo transactionParticipant)
        {
            _committedTransactionParticipants.Add(transactionParticipant);
        }

        /// <summary>
        /// Handle RolledbackParticipantAdded event
        /// </summary>
        /// <param name="transactionParticipant">事务参与方信息</param>
        protected void HandleRolledbackParticipantAddedEvent(TransactionParticipantInfo transactionParticipant)
        {
            _rolledbackTransactionParticipants.Add(transactionParticipant);
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

        #endregion

        #region Participant

        /// <summary>
        /// 支持的事务准备类型列表
        /// </summary>
        protected abstract IEnumerable<Type> SupportedTransactionParticipantTypes { get; }

        /// <summary>
        /// 预提交
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        public void PreCommit(ITransactionPreparation transactionPreparation)
        {
            if (transactionPreparation == null)
            {
                throw new ArgumentNullException(nameof(transactionPreparation));
            }
            if (SupportedTransactionParticipantTypes == null || !SupportedTransactionParticipantTypes.Contains(transactionPreparation.GetType()))
            {
                throw new ApplicationException($"Unknown transaction preparation {transactionPreparation.GetType().Name} for aggregate root {this.GetType().Name}, id={Id}.");
            }
            if (IsTransactionProcessing)
            {
                throw new ApplicationException($"Participant {Id} is already start transaction [{_transactionType}], cann't execute PreCommit command.");
            }
            InternalPreCommit(transactionPreparation);
        }

        /// <summary>
        /// 预提交
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        protected abstract void InternalPreCommit(ITransactionPreparation transactionPreparation);

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        public abstract void Commit(string transactionId);

        /// <summary>
        /// 回滚
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        public abstract void Rollback(string transactionId);

        /// <summary>
        /// 添加事务准备
        /// </summary>
        /// <param name="transactionPreparation">事务准备</param>
        protected void AddTransactionPreparation(ITransactionPreparation transactionPreparation)
        {
            if (_transactionPreparations == null)
            {
                _transactionPreparations = new Dictionary<string, ITransactionPreparation>();
            }
            _transactionPreparations.Add(transactionPreparation.TransactionId, transactionPreparation);
        }

        /// <summary>
        /// 获取事务准备
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        /// <returns></returns>
        protected ITransactionPreparation GetTransactionPreparation(string transactionId)
        {
            return _transactionPreparations != null && _transactionPreparations.ContainsKey(transactionId) ? _transactionPreparations[transactionId] : null;
        }

        /// <summary>
        /// 获取指定类型的事务准备列表
        /// </summary>
        /// <typeparam name="TTransactionPreparation">具体的事务准备</typeparam>
        /// <returns></returns>
        protected IReadOnlyList<TTransactionPreparation> GetTransactionPreparationList<TTransactionPreparation>()
            where TTransactionPreparation : class, ITransactionPreparation
        {
            return _transactionPreparations == null ? new List<TTransactionPreparation>().AsReadOnly() : _transactionPreparations.Values.Select(s => s as TTransactionPreparation).Where(w => w != null).ToList().AsReadOnly();
        }

        /// <summary>
        /// 获取所有事务准备
        /// </summary>
        /// <returns></returns>
        protected IReadOnlyList<ITransactionPreparation> GetAllTransactionPreparations()
        {
            return _transactionPreparations == null ? new List<ITransactionPreparation>().AsReadOnly() : _transactionPreparations.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// 移除事务准备
        /// </summary>
        /// <param name="transactionId">事务ID</param>
        protected void RemoveTransactionPreparation(string transactionId)
        {
            if (_transactionPreparations != null && _transactionPreparations.ContainsKey(transactionId))
            {
                _transactionPreparations.Remove(transactionId);
            }
        }

        /// <summary>
        /// 判断指定的事务准备类型，是否已存在（不同事务流程可能存在互斥，即不能同时存在）
        /// </summary>
        /// <typeparam name="TTransactionPreparation">具体的事务准备</typeparam>
        /// <returns></returns>
        protected bool IsSpecificTransactionPreparationTypeExists<TTransactionPreparation>()
            where TTransactionPreparation : ITransactionPreparation
        {
            return _transactionPreparations != null && _transactionPreparations.Values.Any(a => a.GetType() == typeof(TTransactionPreparation));
        }

        #endregion
    }
}
