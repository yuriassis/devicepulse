# Migração SQLite → PostgreSQL

Faça backup de `devicepulse.db`, crie a organização padrão e aplique as migrations PostgreSQL. Exporte equipamentos e leituras preservando `Id`, valores, timestamps e origem; preencha `OrganizationId` com a organização escolhida, `OfflineTimeoutSeconds=300`, `IsEnabled=true`, `MessageId` com UUID determinístico derivado do tipo e ID legado e converta timestamps para UTC. Valide contagens e valores atuais antes de trocar tráfego.

O utilitário automatizado ainda não está presente; portanto não execute uma migração de produção sem um script revisado e teste de reconciliação. Esta limitação evita documentar como pronto um caminho que ainda não preserva todas as constraints.
