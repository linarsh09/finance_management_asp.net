using FinanceManagement.Filters;

namespace FinanceManagement.Builders;

public class OperationHistoryFilterBuilder
{
    private int? _accountId;
    private DateOnly? _dateFrom;
    private DateOnly? _dateTo;

    public OperationHistoryFilterBuilder ForAccount(int? accountId)
    {
        _accountId = accountId;
        return this;
    }

    public OperationHistoryFilterBuilder FromDate(DateOnly? dateFrom)
    {
        _dateFrom = dateFrom;
        return this;
    }

    public OperationHistoryFilterBuilder ToDate(DateOnly? dateTo)
    {
        _dateTo = dateTo;
        return this;
    }

    public OperationHistoryFilter Build()
    {
        return new OperationHistoryFilter(
            _accountId,
            _dateFrom,
            _dateTo);
    }
}