namespace FundManagement1;

public enum AccountStatus
{
    Active,
    Closed,
    Deactivated
}

public class BankAccount
{
    public string AccountHolder { get; }
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; set; }
    private DateTime? LastTransactionDate { get; set; }
    private readonly TimeSpan DeactivationPeriod = TimeSpan.FromDays(30);
    private readonly Action<string>? _notifier;

    public BankAccount(string accountHolder, Action<string>? notifier = null)
    {
        AccountHolder = accountHolder;
        Status = AccountStatus.Active;
        _notifier = notifier;
    }

    public void Deposit(decimal amount)
    {
        if (Status == AccountStatus.Closed) throw new InvalidOperationException("Cannot deposit to a closed account.");
        if (amount <= 0) throw new ArgumentException("Deposit amount must be positive.");
        Balance += amount;
        LastTransactionDate = DateTime.UtcNow;
        ReactivateIfNeeded();
    }


    public void Withdraw(decimal amount)
    {
        if (Status == AccountStatus.Closed) throw new InvalidOperationException("Cannot withdraw from a closed account.");
        if (Status == AccountStatus.Deactivated) throw new InvalidOperationException("Account is deactivated. Perform a transaction to reactivate.");
        if (amount > Balance) throw new InvalidOperationException("Insufficient funds.");
        if (amount <= 0) throw new ArgumentException("Withdrawal amount must be positive.");

        Balance -= amount;
        LastTransactionDate = DateTime.UtcNow;
        ReactivateIfNeeded();
    }

    public void CloseAccount()
    {
        Status = AccountStatus.Closed;
    }

    public void CheckAndDeactivate()
    {
        if (Status == AccountStatus.Active && LastTransactionDate.HasValue &&
            (DateTime.UtcNow - LastTransactionDate.Value) > DeactivationPeriod)
        {
            Status = AccountStatus.Deactivated;
        }
    }

    private void ReactivateIfNeeded()
    {
        if (Status == AccountStatus.Deactivated)
        {
            Status = AccountStatus.Active;
            _notifier?.Invoke($"Account {AccountHolder} has been reactivated.");
        }
    }

    // Method added for testing
    public void SetLastTransactionDate(DateTime date)
    {
        LastTransactionDate = date;
    }

}
