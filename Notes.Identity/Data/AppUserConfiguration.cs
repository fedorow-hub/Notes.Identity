//интерфейс IEntityTypeConfiguration<Т> позволяет разделять конфигурацию для типа сущностей
//на отдельные классы, а не в методе OnModelCreating ДБ контекста, в котором мы будем просто
//использовать готовую конфигурацию

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notes.Identity.Models;

namespace Notes.Identity.Data;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(x => x.Id);//говорится, что Id это наш ключ
    }
}
