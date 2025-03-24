using FluentAssertions;
using FundManagement1;

public class BankAccountTests
{
    [Fact]
    public void NewAccount_ShouldBeActive_WithZeroBalance()
    {
        var account = new BankAccount("Alice");

        account.Status.Should().Be(AccountStatus.Active);
        account.Balance.Should().Be(0);
    }

    [Fact]
    public void Deposit_ShouldIncreaseBalance()
    {
        var account = new BankAccount("Bob");
        account.Deposit(200);

        account.Balance.Should().Be(200);
    }

    [Fact]
    public void Deposit_ShouldThrowException_WhenAmountIsNegative()
    {
        var account = new BankAccount("Charlie");

        Action act = () => account.Deposit(-100);
        act.Should().Throw<ArgumentException>().WithMessage("Deposit amount must be positive.");
    }

    [Fact]
    public void Withdraw_ShouldDecreaseBalance()
    {
        var account = new BankAccount("David");
        account.Deposit(500);
        account.Withdraw(200);

        account.Balance.Should().Be(300);
    }

    [Fact]
    public void Withdraw_ShouldThrowException_WhenAmountExceedsBalance()
    {
        var account = new BankAccount("Emma");
        account.Deposit(100);

        Action act = () => account.Withdraw(150);
        act.Should().Throw<InvalidOperationException>().WithMessage("Insufficient funds.");
    }

    [Fact]
    public void Withdraw_ShouldThrowException_WhenAccountIsClosed()
    {
        var account = new BankAccount("Frank");
        account.Deposit(300);
        account.CloseAccount();

        Action act = () => account.Withdraw(50);
        act.Should().Throw<InvalidOperationException>().WithMessage("Cannot withdraw from a closed account.");
    }

    [Fact]
    public void Withdraw_ShouldThrowException_WhenAccountIsDeactivated()
    {
        var account = new BankAccount("Grace");
        account.Deposit(300);

        // Simulate inactivity for 31 days
        account.SetLastTransactionDate(DateTime.UtcNow.AddDays(-31));

        account.CheckAndDeactivate();

        Action act = () => account.Withdraw(50);
        act.Should().Throw<InvalidOperationException>().WithMessage("Account is deactivated. Perform a transaction to reactivate.");
    }

    [Fact]
    public void Account_ShouldDeactivate_AfterInactivity()
    {
        var account = new BankAccount("Hannah");
        account.Deposit(100);

        // Simulate inactivity for 31 days
        account.SetLastTransactionDate(DateTime.UtcNow.AddDays(-31));

        account.CheckAndDeactivate();

        account.Status.Should().Be(AccountStatus.Deactivated);
    }

    [Fact]
    public void Deposit_ShouldReactivateAccount()
    {
        string message = "";
        void Notifier(string msg) => message = msg;

        var account = new BankAccount("Isaac", Notifier);
        account.Deposit(100);

        // Simulate inactivity for 31 days
        account.SetLastTransactionDate(DateTime.UtcNow.AddDays(-31));

        account.CheckAndDeactivate();
        account.Status.Should().Be(AccountStatus.Deactivated);

        account.Deposit(50); // Reactivate with deposit

        account.Status.Should().Be(AccountStatus.Active);
        message.Should().Be("Account Isaac has been reactivated.");
    }

    [Fact]
    public void CloseAccount_ShouldPreventFurtherTransactions()
    {
        var account = new BankAccount("Jack");
        account.Deposit(200);
        account.CloseAccount();

        account.Status.Should().Be(AccountStatus.Closed);

        Action depositAct = () => account.Deposit(50);
        depositAct.Should().Throw<InvalidOperationException>().WithMessage("Cannot deposit to a closed account.");

        Action withdrawAct = () => account.Withdraw(50);
        withdrawAct.Should().Throw<InvalidOperationException>().WithMessage("Cannot withdraw from a closed account.");
    }
}
