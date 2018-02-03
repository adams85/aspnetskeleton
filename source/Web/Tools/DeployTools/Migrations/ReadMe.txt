Adding a new migration:
1. Add <migration-name> to the list returned by MigrationHistory property of <context-name> class inherited from DbContext<TContext>
2. Create migration sql script at <context-name>\<db-provider-name>.<migration-name>.up.sql
3. Create undo sql script at <context-name>\<db-provider-name>.<migration-name>.down.sql
