using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class CirculationService : ICirculationService
    {
        private readonly ICopyRepository _copyRepository;
        private readonly IReaderRepository _readerRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IValidationService _validationService;

        public CirculationService(
            ICopyRepository copyRepository,
            IReaderRepository readerRepository,
            ILoanRepository loanRepository,
            IValidationService validationService)
        {
            _copyRepository = copyRepository;
            _readerRepository = readerRepository;
            _loanRepository = loanRepository;
            _validationService = validationService;
        }

        public async Task<IssueResult> IssueCopyAsync(int copyId, int readerId, int days)
        {
            var copy = await _copyRepository.GetByIdAsync(copyId);
            var reader = await _readerRepository.GetByIdAsync(readerId);

            if (copy is null || reader is null)
                return IssueResult.NotFound;

            if (!_validationService.CanIssueToReader(reader))
                return IssueResult.ReaderBlocked;

            if (copy.Status != CopyStatus.Available)
                return IssueResult.CopyNotAvailable;

            var now = DateTime.Now;
            var dueDate = now.AddDays(days);

            var loan = new Loan
            {
                CopyId = copyId,
                ReaderId = readerId,
                IssueDate = now,
                DueDate = dueDate,
                FineAmount = 0m
            };

            copy.Status = CopyStatus.OnLoan;

            await _loanRepository.AddAsync(loan);
            await _copyRepository.UpdateAsync(copy);

            return IssueResult.Success;
        }

        public async Task<ReturnResult> ReturnCopyAsync(int copyId)
        {
            var copy = await _copyRepository.GetByIdAsync(copyId);
            if (copy is null)
                return ReturnResult.NotFound;

            var loan = await _loanRepository.GetActiveLoanByCopyIdAsync(copyId);
            if (loan is null)
                return ReturnResult.NoActiveLoan;

            var now = DateTime.Now;
            loan.ReturnDate = now;
            loan.FineAmount = _validationService.CalculateFine(loan.DueDate, now);

            copy.Status = CopyStatus.Available;

            await _loanRepository.UpdateAsync(loan);
            await _copyRepository.UpdateAsync(copy);

            return ReturnResult.Success;
        }
    }

}
