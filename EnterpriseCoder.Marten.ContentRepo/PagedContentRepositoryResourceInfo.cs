using System.Collections;
using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

/// <summary>
/// The <c>PagedContentRepositoryResourceInfo</c> class is a wrapper around a <c>IPagedList</c> that
/// contains a list of <c>ContentRepositoryResourceInfo</c> objects.  This class is used to return
/// a paged listing of resources from the repository.
/// </summary>
public class PagedContentRepositoryResourceInfo : IPagedList<ContentRepositoryResourceInfo>
{
    private readonly List<ContentRepositoryResourceInfo> _items;

    public PagedContentRepositoryResourceInfo(IPagedList<ContentFileHeader> items, string bucketName)
    {
        var itemList = new List<ContentRepositoryResourceInfo>();
        foreach (var nextItem in items)
        {
            itemList.Add(nextItem.ToContentFileInfoDto(bucketName));
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

    public IEnumerator<ContentRepositoryResourceInfo> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ContentRepositoryResourceInfo this[int index] => _items[index];

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