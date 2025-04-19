using System.Collections;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

/// <summary>
/// The <c>PagedBucketNameListing</c> class is a wrapper around a <c>IPagedList</c> that
/// contains a list of bucket names.  This class is used to return a paged listing of buckets
/// from the repository.
/// </summary>
public class PagedBucketNameListing : IPagedList<string>
{
    private readonly List<string> _items;

    internal PagedBucketNameListing(IPagedList<ContentBucket> items)
    {
        _items = new List<string>(items.Select(x => x.BucketName));
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

    public IEnumerator<string> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public string this[int index] => _items[index];

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