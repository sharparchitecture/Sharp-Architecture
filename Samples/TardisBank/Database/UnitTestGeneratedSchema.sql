
    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_Messages_For_Users]') and parent_object_id = OBJECT_ID(N'Messages'))
alter table Messages  drop constraint FK_Messages_For_Users


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_PaymentSchedules_For_Accounts]') and parent_object_id = OBJECT_ID(N'PaymentSchedules'))
alter table PaymentSchedules  drop constraint FK_PaymentSchedules_For_Accounts


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_Transactions_For_Accounts]') and parent_object_id = OBJECT_ID(N'Transactions'))
alter table Transactions  drop constraint FK_Transactions_For_Accounts


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_Children_Join_Users]') and parent_object_id = OBJECT_ID(N'Children'))
alter table Children  drop constraint FK_Children_Join_Users


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_Children_Ref_Accounts]') and parent_object_id = OBJECT_ID(N'Children'))
alter table Children  drop constraint FK_Children_Ref_Accounts


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_Childrens_For_Parents]') and parent_object_id = OBJECT_ID(N'Children'))
alter table Children  drop constraint FK_Childrens_For_Parents


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_Parents_Join_Users]') and parent_object_id = OBJECT_ID(N'Parents'))
alter table Parents  drop constraint FK_Parents_Join_Users


    if exists (select * from dbo.sysobjects where id = object_id(N'Accounts') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Accounts

    if exists (select * from dbo.sysobjects where id = object_id(N'Announcements') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Announcements

    if exists (select * from dbo.sysobjects where id = object_id(N'Messages') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Messages

    if exists (select * from dbo.sysobjects where id = object_id(N'PaymentSchedules') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table PaymentSchedules

    if exists (select * from dbo.sysobjects where id = object_id(N'Transactions') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Transactions

    if exists (select * from dbo.sysobjects where id = object_id(N'Users') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Users

    if exists (select * from dbo.sysobjects where id = object_id(N'Children') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Children

    if exists (select * from dbo.sysobjects where id = object_id(N'Parents') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Parents

    if exists (select * from dbo.sysobjects where id = object_id(N'hibernate_unique_key') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table hibernate_unique_key

    create table Accounts (
        AccountId INT not null,
       OldTransactionsBalance DECIMAL(19,5) null,
       primary key (AccountId)
    )

    create table Announcements (
        AnnouncementId INT not null,
       LastModifiedUtc DATETIME2 null,
       Date DATETIME2 null,
       Title NVARCHAR(255) null,
       Content NVARCHAR(255) null,
       primary key (AnnouncementId)
    )

    create table Messages (
        MessageId INT not null,
       Date DATETIME2 null,
       Text NVARCHAR(255) null,
       HasBeenRead BIT null,
       UserId INT null,
       primary key (MessageId)
    )

    create table PaymentSchedules (
        PaymentScheduleId INT not null,
       NextRun DATETIME2 null,
       Interval NVARCHAR(255) null,
       Amount DECIMAL(19,5) null,
       Description NVARCHAR(255) null,
       AccountId INT null,
       primary key (PaymentScheduleId)
    )

    create table Transactions (
        TransactionId INT not null,
       Description NVARCHAR(255) null,
       Amount DECIMAL(19,5) null,
       Date DATETIME2 null,
       AccountId INT null,
       primary key (TransactionId)
    )

    create table Users (
        UserId INT not null,
       Name NVARCHAR(255) null,
       UserName NVARCHAR(255) null,
       Password NVARCHAR(255) null,
       IsActive BIT null,
       primary key (UserId)
    )

    create table Children (
        UserId INT not null,
       ParentId INT null,
       AccountId INT null,
       primary key (UserId)
    )

    create table Parents (
        UserId INT not null,
       ActivationKey NVARCHAR(255) null,
       primary key (UserId)
    )

    alter table Messages 
        add constraint FK_Messages_For_Users 
        foreign key (UserId) 
        references Users

    alter table PaymentSchedules 
        add constraint FK_PaymentSchedules_For_Accounts 
        foreign key (AccountId) 
        references Accounts

    alter table Transactions 
        add constraint FK_Transactions_For_Accounts 
        foreign key (AccountId) 
        references Accounts

    alter table Children 
        add constraint FK_Children_Join_Users 
        foreign key (UserId) 
        references Users

    alter table Children 
        add constraint FK_Children_Ref_Accounts 
        foreign key (AccountId) 
        references Accounts

    alter table Children 
        add constraint FK_Childrens_For_Parents 
        foreign key (ParentId) 
        references Parents

    alter table Parents 
        add constraint FK_Parents_Join_Users 
        foreign key (UserId) 
        references Users

    create table hibernate_unique_key (
         next_hi INT 
    )

    insert into hibernate_unique_key values ( 1 )
