using System.Collections;
using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

public class PagedContentRepositoryFileInfo : IPagedList<ContentRepositoryFileInfo>
{
    private readonly List<ContentRepositoryFileInfo> _items;

    public PagedContentRepositoryFileInfo(IPagedList<ContentFileHeader> items)
    {
        var itemList = new List<ContentRepositoryFileInfo>();
        foreach (var nextItem in items)
        {
            itemList.Add(nextItem.ToContentFileInfoDto());
        }

        _items = itemList;
        Count = items.Count;
        PageNumber = items.PageNumber;
        PageSize = items.PageSize;
        PageCount = items.PageCount;
        TotalItemCount = items.TotalItemCount;
        HasPreviousPage = items.HasPreviousPage;
        HasNextPage = items.HasNextPage;
        IsFirstPage = items.IsFirstPage;
        IsLastPage = items.IsLastPage;
        FirstItemOnPage = items.FirstItemOnPage;
        LastItemOnPage = items.LastItemOnPage;
    }

    public IEnumerator<ContentRepositoryFileInfo> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ContentRepositoryFileInfo this[int index] => _items[index];

    public long Count { get; }
    public long PageNumber { get; }
    public long PageSize { get; }
    public long PageCount { get; }
    public long TotalItemCount { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
    public bool IsFirstPage { get; }
    public bool IsLastPage { get; }
    public long FirstItemOnPage { get; }
    public long LastItemOnPage { get; }
}