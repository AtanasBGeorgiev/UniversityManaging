using System;
using System.Linq;
using System.Linq.Expressions;
using Common.Entities;
using Common.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Common.Services;

public class BaseService<T>
where T : BaseEntity
{
    private DbContext Context { get; set; }
    private DbSet<T> Items { get; set; }

    public BaseService()
    {
        Context = new AppDbContext();
        Items = Context.Set<T>();
    }

    public List<T> GetAll(Expression<Func<T, bool>> filter = null, string orderBy = null, bool sortAsc = false, int page = 1, int pageSize = int.MaxValue)
    {
        var query = Items.AsQueryable();

        if (filter != null)
            query = query.Where(filter);

        if (!string.IsNullOrEmpty(orderBy))
        {
            if (sortAsc)
                query = query.OrderBy(e => EF.Property<object>(e, orderBy));//takes the property value
            else
                query = query.OrderByDescending(e => EF.Property<object>(e, orderBy));
        }

        query = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize);

        return query.ToList();
    }
    public T GetByProperty(Expression<Func<T, bool>> filter)
    {
        var query = Items.AsQueryable();
        if (filter != null)
        {
            query = query.Where(filter);
        }

        return query.FirstOrDefault();
    }

    public int Count(Expression<Func<T, bool>> filter = null)
    {
        var query = Items.AsQueryable();
        if (filter != null)
        {
            query = query.Where(filter);
        }

        return query.Count();
    }
    public T GetById(int id, int secondId = 0)
    {
        if (secondId == 0)
            return Items.Find(id);
        else
            return Items.Find(id, secondId);
    }
    public void Save(T item, int oldSecondId = 0, bool update = false)
    {
        int[] PK = item.GetIds();

        if (PK.Length == 1 && PK[0] == 0)//if has one attribute in PK and item is new
        {
            Items.Add(item);
        }
        else if (PK.Length == 1 && PK[0] != 0)//if has one attribute in PK and item is for update
        {
            Items.Update(item);
        }
        else if (PK.Length == 2)//if has two attributes in PK
        {
            BaseService<T> composite = new BaseService<T>();

            if (oldSecondId != 0)//if has update in the composite key
            {
                var oldItem = composite.GetById(PK[0], oldSecondId);
                if (oldItem != null)//item contains new second id
                {
                    //EF does not allow replacement in a composite PK
                    Items.Remove(oldItem);
                    Items.Add(item);
                }
            }
            else if (update == true)//update out of composite key
                Items.Update(item);
            else
                Items.Add(item);
        }

        Context.SaveChanges();
    }

    public void Delete(T item)
    {
        Items.Remove(item);
        Context.SaveChanges();
    }
}