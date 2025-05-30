using ContactUser.Application.Dtos;

namespace ContactUser.Application.Services;

public interface IContactService
{
    Task<long> AddContactAsync(ContactCreateDto contactCreateDto, long userId);
    Task<ContactDto> GetContactByIdAsync(long contactId, long userId);
    Task<List<ContactDto>> GetAllContactsAsync(long userId);
    Task DeleteContactAsync(long contactId, long userId);
    Task UpdateContactAsync(ContactDto contactDto, long userId);
}