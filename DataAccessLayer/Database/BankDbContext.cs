using System;
using System.Collections.Generic;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace DataAccessLayer.Database;

public class BankDbContext : DbContext
{
    public DbSet<Bank> Banks { get; set; }

    public DbSet<BankAccount> BankAccounts { get; set; }

    public DbSet<Client> Clients { get; set; }

    public DbSet<Credit> Credits { get; set; }

    public DbSet<Deposit> Deposits { get; set; }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<Log> Logs { get; set; }

    public DbSet<Passport> Passports { get; set; }

    public DbSet<Transaction> Transactions { get; set; }

    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bank>(bank =>
        {
            bank.HasIndex(b => b.Address, "UQ_banks_address").IsUnique();
            bank.HasIndex(b => b.BIC, "UQ_banks_BIC").IsUnique();
            bank.HasIndex(b => b.Title, "UQ_banks_title").IsUnique();
            bank.Property(b => b.BIC).IsFixedLength();
        });

        modelBuilder.Entity<BankAccount>(bankAccount =>
        {
            bankAccount.HasIndex(ba => ba.BankAccountNumber, "UQ_bank_account_bank_account_number").IsUnique();
            bankAccount.Property(ba => ba.BankAccountNumber).IsFixedLength();
            bankAccount.Property(ba => ba.Currency).HasConversion<string>().IsFixedLength();
            bankAccount.Property(ba => ba.Status).HasConversion<string>();
            bankAccount.Property(ba => ba.Type).HasConversion<string>();
            bankAccount.Property(ba => ba.MoneyBalance).HasColumnType("decimal(18, 4)");
            bankAccount
                .HasOne(ba => ba.Bank)
                .WithMany(b => b.BankAccounts)
                .HasForeignKey(ba => ba.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            bankAccount
                .HasOne(ba => ba.Client)
                .WithMany(b => b.BankAccounts)
                .HasForeignKey(ba => ba.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Client>(client =>
        {
            client.HasIndex(cl => cl.PhoneNumber, "UQ_clients_phone_number").IsUnique();
            client.Property(cl => cl.PhoneNumber).IsFixedLength();
            client.Property(cl => cl.Status).HasConversion<string>();

            client.HasOne(cl => cl.Passport)
            .WithOne(p => p.Client)
            .HasForeignKey<Client>(cl => cl.PassportId)
            .OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<Credit>(credit =>
        {
            credit.Property(cr => cr.LoanAmount).HasColumnType("decimal(18, 4)");
            credit.Property(cr => cr.LoanBalance).HasColumnType("decimal(18, 4)");
            credit.Property(cr => cr.LoanInterest).HasColumnType("decimal(18, 4)");
            credit.Property(cr => cr.Status).HasConversion<string>();
            credit.Property(cr => cr.Currency).HasConversion<string>();
            credit.ToTable(cr => cr.HasCheckConstraint("CK_Credit_LoanInterest", "LoanInterest >= 14 AND LoanInterest <= 25"));

            credit.HasOne(cr => cr.Bank).WithMany(b => b.Credits)
                .HasForeignKey(cr => cr.BankId);

            credit.HasOne(cr => cr.Client).WithMany(b => b.Credits)
                .HasForeignKey(cr => cr.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Deposit>(deposit =>
        {
            deposit.Property(d => d.DepositAmount).HasColumnType("decimal(18, 4)");
            deposit.Property(d => d.Status).HasConversion<string>();
            deposit.Property(d => d.Currency).HasConversion<string>();

            deposit.HasOne(d => d.Bank).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.BankId);

            deposit.HasOne(d => d.Client).WithMany(cl => cl.Deposits)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Employee>(employee =>
        {
            employee.HasIndex(emp => emp.PhoneNumber, "UQ_employee_phone_number").IsUnique();
            employee.HasIndex(emp => emp.PersonellNumber, "UQ_employee_personell_number").IsUnique();
            employee.Property(emp => emp.PersonellNumber).IsFixedLength();
            employee.Property(emp => emp.PhoneNumber).IsFixedLength();

            employee.HasOne(emp => emp.Bank).WithMany(b => b.Employees)
                .HasForeignKey(emp => emp.BankId)
                .OnDelete(DeleteBehavior.Cascade);
            
            employee.HasOne(emp => emp.Passport).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(emp => emp.PassportId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Log>(log =>
        {
            log.HasOne(l => l.Employee).WithMany(emp => emp.Logs)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Passport>(passport =>
        {
            passport.HasIndex(p => p.IdentificationNumber, "UQ_passport_identification_number").IsUnique();
            passport.HasIndex(p => p.Number, "UQ_passport_number").IsUnique();
            passport.Property(p => p.IdentificationNumber).IsFixedLength();
            passport.Property(p => p.Number).IsFixedLength();
            passport.Property(p => p.Series).IsFixedLength();
        });

        modelBuilder.Entity<Transaction>(transaction =>
        {
            transaction.Property(tr => tr.Currency).HasConversion<string>().IsFixedLength();
            transaction.Property(tr => tr.TransactionAmount).HasColumnType("decimal(18, 4)");
            transaction.Property(tr => tr.Status).HasConversion<string>();
            transaction.Property(tr => tr.Type).HasConversion<string>();

            transaction.HasOne(tr => tr.Receiver).WithMany(ba => ba.TransactionReceivers)
                .HasForeignKey(tr => tr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            transaction.HasOne(tr => tr.Sender).WithMany(ba => ba.TransactionSenders)
                .HasForeignKey(tr => tr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
