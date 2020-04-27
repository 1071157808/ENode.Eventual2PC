# ENode.Eventual2PC

基于Eventual2PC的最终一致性二阶段提交实现，用于ENode。

最终一致性二阶段提交范式，参见 [Eventual2PC](https://github.com/berkaroad/Eventual2PC)

## 安装

```
dotnet add package ENode.Eventual2PC
```

## 用法

```csharp
// 银行账号（转账事务的参与者）
public BandAccount : ENode.Eventual2PC.TransactionParticipantBase<string>
{
    // 抽象方法实现
}

// 转账事务（转账事务的发起者）
public TransferTransaction : ENode.Eventual2PC.TransactionInitiatorBase<TransferTransaction, string>
{
    // 抽象方法实现
}
```

## 发布历史

## 1.1.0

- 1）`TransactionInitiatorBase` 添加校验逻辑，以符合 `Eventual2PC` 中的规约描述

- 2）新增 `TransactionInitiatorAlsoActAsOtherParticipantBase`，以满足一个聚合根实例既是事务A的 `Initiator`， 又是事务B的 `Participant` 的场景

- 3）小重构，将事件接口替换为抽象类，目的是为了减少使用方的编码量

## 1.0.5

- 1）修复 `TransactionInitiatorBase`、 `TransactionParticipantBase`内部处理

## 1.0.4

- 1）修复 `TransactionParticipantBase`内部处理


## 1.0.3

- 1）修复 `TransactionInitiatorBase`


## 1.0.2

- 1）取消自定义接口 `ITransactionPreparation`

## 1.0.1

- 1）增加 `PreCommitFailed` 的领域异常接口 `ITransactionParticipantPreCommitDomainException`

## 1.0.0

- 初始版本