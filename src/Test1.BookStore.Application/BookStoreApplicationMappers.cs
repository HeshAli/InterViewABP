using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Test1.BookStore.Books;

namespace Test1.BookStore;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BookStoreBookToBookDtoMapper : MapperBase<Book, BookDto>
{
    public override partial BookDto Map(Book source);

    public override partial void Map(Book source, BookDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BookStoreCreateUpdateBookDtoToBookMapper : MapperBase<CreateUpdateBookDto, Book>
{
    public override partial Book Map(CreateUpdateBookDto source);

    public override partial void Map(CreateUpdateBookDto source, Book destination);
}
