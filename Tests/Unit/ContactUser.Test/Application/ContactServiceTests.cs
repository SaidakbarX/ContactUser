using Xunit;
using Moq;
using FluentValidation;
using FluentValidation.Results;
using ContactUser.Application.Services;
using ContactUser.Application.Interfaces;
using ContactUser.Application.Dtos;
using ContactUser.Domain.Entities;
using ContactUser.Core.Errors;

public class ContactServiceTests
{
    private readonly Mock<IContactRepository> _contactRepoMock = new();
    private readonly Mock<IValidator<ContactCreateDto>> _createDtoValidatorMock = new();
    private readonly Mock<IValidator<ContactDto>> _updateDtoValidatorMock = new();

    private ContactService CreateService() =>
        new(_contactRepoMock.Object, _createDtoValidatorMock.Object, _updateDtoValidatorMock.Object);

    [Fact]
    public async Task AddContactAsync_ValidDto_ReturnsContactId()
    {
        var dto = new ContactCreateDto
        {
            FirstName = "Ali",
            LastName = "Valiyev",
            Email = "ali@mail.com",
            PhoneNumber = "998901112233",
            Address = "Tashkent"
        };

        _createDtoValidatorMock.Setup(v => v.Validate(dto)).Returns(new ValidationResult());

        _contactRepoMock.Setup(r => r.AddContactAsync(It.IsAny<Contact>()))
                        .ReturnsAsync(1);

        var service = CreateService();

        var result = await service.AddContactAsync(dto, userId: 10);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task AddContactAsync_InvalidDto_ThrowsNotAllowedException()
    {
        var dto = new ContactCreateDto();

        var validationErrors = new List<ValidationFailure>
        {
            new("FirstName", "First name required")
        };

        _createDtoValidatorMock.Setup(v => v.Validate(dto)).Returns(new ValidationResult(validationErrors));

        var service = CreateService();

        await Assert.ThrowsAsync<NotAllowedException>(() => service.AddContactAsync(dto, 10));
    }

    [Fact]
    public async Task DeleteContactAsync_CallsRepository()
    {
        _contactRepoMock.Setup(r => r.DeleteContactAsync(1, 10)).Returns(Task.CompletedTask).Verifiable();

        var service = CreateService();

        await service.DeleteContactAsync(1, 10);

        _contactRepoMock.Verify(r => r.DeleteContactAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task GetAllContactsAsync_ReturnsContactList()
    {
        var contacts = new List<Contact>
        {
            new Contact
            {
                Id = 1,
                FirstName = "Ali",
                LastName = "Valiyev",
                Email = "ali@mail.com",
                PhoneNumber = "998901112233",
                Address = "Tashkent"
            },
            new Contact
            {
                Id = 2,
                FirstName = "Vali",
                LastName = "Aliyev",
                Email = "vali@mail.com",
                PhoneNumber = "998901122233",
                Address = "Samarkand"
            }
        };

        _contactRepoMock.Setup(r => r.GetAllContactsAsync(10)).ReturnsAsync(contacts);

        var service = CreateService();

        var result = await service.GetAllContactsAsync(10);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == 1 && r.FirstName == "Ali");
        Assert.Contains(result, r => r.Id == 2 && r.FirstName == "Vali");
    }

    [Fact]
    public async Task GetContactByIdAsync_ReturnsContact()
    {
        var contact = new Contact
        {
            Id = 1,
            FirstName = "Ali",
            LastName = "Valiyev",
            Email = "ali@mail.com",
            PhoneNumber = "998901112233",
            Address = "Tashkent"
        };

        _contactRepoMock.Setup(r => r.GetContactByIdAsync(1, 10)).ReturnsAsync(contact);

        var service = CreateService();

        var result = await service.GetContactByIdAsync(1, 10);

        Assert.Equal("Ali", result.FirstName);
        Assert.Equal("Valiyev", result.LastName);
    }

    [Fact]
    public async Task UpdateContactAsync_ValidDto_UpdatesContact()
    {
        var dto = new ContactDto
        {
            Id = 1,
            FirstName = "Ali",
            LastName = "Valiyev",
            Email = "ali@mail.com",
            PhoneNumber = "998901112233",
            Address = "New address"
        };

        var contactFromDb = new Contact
        {
            Id = 1,
            FirstName = "Old",
            LastName = "Name",
            Email = "old@mail.com",
            PhoneNumber = "0000000",
            Address = "Old Address"
        };

        _updateDtoValidatorMock.Setup(v => v.Validate(dto)).Returns(new ValidationResult());

        _contactRepoMock.Setup(r => r.GetContactByIdAsync(dto.Id, 10)).ReturnsAsync(contactFromDb);
        _contactRepoMock.Setup(r => r.UpdateContactAsync(It.IsAny<Contact>())).Returns(Task.CompletedTask).Verifiable();

        var service = CreateService();

        await service.UpdateContactAsync(dto, 10);

        _contactRepoMock.Verify(r => r.UpdateContactAsync(It.Is<Contact>(c =>
            c.Email == "ali@mail.com" &&
            c.FirstName == "Ali" &&
            c.LastName == "Valiyev" &&
            c.Address == "New address"
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateContactAsync_InvalidDto_ThrowsNotAllowed()
    {
        var dto = new ContactDto();

        var validationErrors = new List<ValidationFailure>
        {
            new("Email", "Email is required")
        };

        _updateDtoValidatorMock.Setup(v => v.Validate(dto)).Returns(new ValidationResult(validationErrors));

        var service = CreateService();

        await Assert.ThrowsAsync<NotAllowedException>(() => service.UpdateContactAsync(dto, 10));
    }
}
