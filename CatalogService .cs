using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class CatalogService : ICatalogService
    {
        private readonly ILibraryItemRepository _libraryItemRepository;
        private readonly ICopyRepository _copyRepository;
        private readonly IValidationService _validationService;

        public CatalogService(
            ILibraryItemRepository libraryItemRepository,
            ICopyRepository copyRepository,
            IValidationService validationService)
        {
            _libraryItemRepository = libraryItemRepository;
            _copyRepository = copyRepository;
            _validationService = validationService;
        }

        public async Task<int?> AddLibraryItemAsync(LibraryItemDto dto)
        {
            if (!_validationService.ValidateIsbn(dto.Isbn) ||
                string.IsNullOrWhiteSpace(dto.Title))
                return null;

            var item = new LibraryItem
            {
                Title = dto.Title.Trim(),
                Authors = dto.Authors?.Trim(),
                Year = dto.Year,
                Isbn = dto.Isbn,
                Udk = dto.Udk,
                Bbk = dto.Bbk,
                ItemType = dto.ItemType,
                SubjectAreaId = dto.SubjectAreaId
            };

            await _libraryItemRepository.AddAsync(item);
            return item.Id;
        }

        public Task<IEnumerable<LibraryItem>> SearchAsync(string query) =>
            _libraryItemRepository.SearchAsync(query);

        public Task<LibraryItem?> GetByIdAsync(int id) =>
            _libraryItemRepository.GetByIdAsync(id);
    }

}
