####################
UnitOfWork attribute
####################

With the RavenDB client, changes are not persisted until SaveChanges is called on the RavenDB session. With S#arp Architecture's implementaion of the RavenDB repository, the changes are not persisted on each call, in order to allow for multiple changes to happen per request with all of them being atomic, so that a later error within the request would not leave you application in an inconsitant state.

The UnitOfWorkAttribute should be added to all controller actions where changes are being made to RavenDB, otherwise the changes would not be persisted (unless you get hold of the session and call save changed youself).

RavenDB provides does provide DTC transactions, but the UnitOfWork attribute only uses RavenDB's standard transactions based on the SaveChanges call. 

If you want more information about transactions in RavenDB, refer to `RavenDB transaction support page <http://ravendb.net/docs/client-api/advanced/transaction-support>`_.
