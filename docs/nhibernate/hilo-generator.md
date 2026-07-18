# NHibernate HiLo Generator

## Why use HiLo

HiLo identity generators let NHibernate assign identity values to child objects without
hitting the database on every save. The native identity generation strategy, by
contrast, makes NHibernate hit the database on every insert to obtain the next id,
which hurts performance and batching.

For more background on generator strategies, see
[NH2.1.0 - Generators behavior explained](https://fabiomaulo.blogspot.com/2009/02/nh210-generators-behavior-explained.html)
by Fabio Maulo.

## Using HiLo id generation

Add a Fluent NHibernate `IIdConvention` to your infrastructure/mapping project. This is
the actual convention used by the [`TardisBank`](../../Src/Samples/TardisBank) sample
(`Suteki.TardisBank.Infrastructure/NHibernateMaps/Conventions/PrimaryKeyConvention.cs`):

```csharp
public class PrimaryKeyConvention : IIdConvention
{
    public void Apply(IIdentityInstance instance)
    {
        instance.Column(instance.EntityType.Name + "Id");
        instance.GeneratedBy.HiLo("10");
    }
}
```

Then create the table NHibernate uses to allocate "hi" blocks of ids:

```sql
CREATE TABLE [dbo].[hibernate_unique_key](
    [next_hi] [int] NOT NULL
) ON [PRIMARY]
```

Seed the table with a starting value (e.g. `1`) and you're done — NHibernate will
allocate ids in blocks, only round-tripping to the database once per block instead of
once per insert.
