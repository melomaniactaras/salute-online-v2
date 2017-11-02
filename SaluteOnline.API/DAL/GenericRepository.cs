﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SaluteOnline.Domain.Common;
using SaluteOnline.Domain.Domain;
using SaluteOnline.Domain.Extensions;

namespace SaluteOnline.API.DAL
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity: class, IEntity
    {
        private readonly SaluteOnlineDbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly IMongoDatabase _mongoDb;
        public GenericRepository(SaluteOnlineDbContext context, IConfiguration configuration)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            if (typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                _mongoDb = new MongoClient(configuration.GetValue<string>("MongoSettings:Path"))
                    .GetDatabase(configuration.GetValue<string>("MongoSettings:DB"));
            }
            else
            {
                _context = context;
                _dbSet = context.Set<TEntity>();
            }
        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "", bool ascending = false)
        {
            if (typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                var collection = filter == null
                    ? _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName()).Find(_ => true).ToList()
                    : _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                        .Find(Builders<TEntity>.Filter.Where(filter))
                        .ToList();
                return orderBy?.Invoke(collection.AsQueryable()).ToList() ?? collection;
            }
            var query = GetGeneric(filter, includeProperties, orderBy, ascending);
            return orderBy?.Invoke(query).ToList() ?? query.ToList();
        }

        public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "", bool ascending = false)
        {

            if (typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                var type = typeof(TEntity).ToMongoCollectionName();
                var collection = filter == null
                    ? _mongoDb.GetCollection<TEntity>(type).Find(_ => true)
                    : _mongoDb.GetCollection<TEntity>(type).Find(Builders<TEntity>.Filter.Where(filter));
                return await collection.ToListAsync();
            }
            var query = GetGeneric(filter, includeProperties, orderBy, ascending);
            if (orderBy != null)
            {
                return await orderBy.Invoke(query).ToListAsync();
            }
            return await query.ToListAsync();
        }

        public IQueryable<TEntity> GetAsQueryable(Expression<Func<TEntity, bool>> filter = null)
        {
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
                return filter != null ? _dbSet.Where(filter) : _dbSet;
            var type = typeof(TEntity).ToMongoCollectionName();
            var collection = filter == null
                ? _mongoDb.GetCollection<TEntity>(type).AsQueryable()
                : _mongoDb.GetCollection<TEntity>(type).AsQueryable().Where(filter);
            return collection;
        }

        public Page<TEntity> GetPage(int page, int items, Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "", bool ascending = false)
        {
            if (typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                var type = typeof(TEntity).ToMongoCollectionName();
                var allCollection = filter == null
                    ? _mongoDb.GetCollection<TEntity>(type).Find(_ => true)
                    : _mongoDb.GetCollection<TEntity>(type)
                        .Find(Builders<TEntity>.Filter.Where(filter));
                var collection = allCollection.SortBy(t => orderBy).Skip((page - 1) * items).Limit(items);
                return new Page<TEntity>(page, (int)collection.Count(), allCollection.Count(), collection.ToList());
            }
            var needed = GetGeneric(filter, includeProperties, orderBy, ascending);
            var all = needed.Count();
            var slice = needed.Skip((page - 1) * items).Take(items).ToList();
            return new Page<TEntity>(page, items, all, slice);
        }

        public async Task<Page<TEntity>> GetPageAsync(int page, int items, Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "", bool ascending = false)
        {
            if (typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                var type = typeof(TEntity).ToMongoCollectionName();
                var allCollection = filter == null
                    ?  await _mongoDb.GetCollection<TEntity>(type).Find(_ => true).ToListAsync()
                    : await _mongoDb.GetCollection<TEntity>(type)
                        .Find(Builders<TEntity>.Filter.Where(filter)).ToListAsync();
                var collection = allCollection.OrderBy(t => orderBy).Skip((page - 1) * items).Take(items).ToList();
                return new Page<TEntity>(page, collection.Count, allCollection.Count, collection.ToList());
            }
            var query = GetGeneric(filter, includeProperties, orderBy, ascending);
            var slice = query.Skip((page - 1) * items).Take(items).ToList();
            return new Page<TEntity>(page, slice.Count, query.Count(), slice);
        }

        public TEntity GetById(Guid guid = default(Guid), int? id = null)
        {
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity))) return id != null ? _dbSet.SingleOrDefault(t => t.Id == id) : null;
            return guid != Guid.Empty ? _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                    .Aggregate()
                    .Match(t => t.Guid == guid)
                    .SingleOrDefault() : null;
        }

        public async Task<TEntity> GetByIdAsync(Guid guid = default(Guid), int? id = null)
        {
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity))) return id != null ? await _dbSet.SingleOrDefaultAsync(t => t.Id == id) : null;
            return guid != Guid.Empty ? await _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                .Aggregate()
                .Match(t => t.Guid == guid)
                .FirstOrDefaultAsync() : null;
        }

        public void Insert(TEntity entity)
        {
            if (entity == null) throw new InvalidOperationException("Unable to add a null entity to the repository.");
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity))) _dbSet.Add(entity);
            else _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName()).InsertOne(entity);
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new InvalidOperationException("Unable to add a null entity to the repository.");
            }
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                _dbSet.Add(entity);
                return entity;
            }
            await _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName()).InsertOneAsync(entity);
            return entity;
        }

        public void Delete(Guid guid = default(Guid), int? id = null)
        {
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)) && id != null)
            {
                var entityToDelete = _dbSet.SingleOrDefault(t => t.Id == id);
                Delete(entityToDelete);
            }
            if (guid != Guid.Empty)
            {
                _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
               .FindOneAndDelete(t => t.Guid == guid);
            }
        }

        public async void DeleteAsync(Guid guid = default(Guid), int? id = null)
        {
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)) && id != null)
            {
                var entityToDelete = await _dbSet.SingleOrDefaultAsync(t => t.Id == id);
                Delete(entityToDelete);
            }
            if (guid != Guid.Empty)
            {
                await _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                    .FindOneAndDeleteAsync(t => t.Id == id);
            }
        }

        public void Delete(TEntity entityToDelete)
        {
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                if (_context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    _dbSet.Attach(entityToDelete);
                }
                _dbSet.Remove(entityToDelete);
            }
            _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                .FindOneAndDelete(t => t.Guid == entityToDelete.Guid);
        }

        public async void DeleteAsync(TEntity entityToDelete)
        {
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                if (_context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    _dbSet.Attach(entityToDelete);
                }
                _dbSet.Remove(entityToDelete);
            }
            await _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                .FindOneAndDeleteAsync(t => t.Guid == entityToDelete.Guid);
        }

        public TEntity Update(TEntity entityToUpdate)
        {
            if (entityToUpdate == null)
            {
                throw new InvalidOperationException("Unable to update a null entity in the repository.");
            }
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                _dbSet.Attach(entityToUpdate);
                _context.Entry(entityToUpdate).State = EntityState.Modified;
                return entityToUpdate;
            }
            var result = _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                    .ReplaceOne(t => t.Guid == entityToUpdate.Guid,
                        entityToUpdate);
            return result.IsAcknowledged ? entityToUpdate : null;
        }

        public async Task<TEntity> UpdateAsync(TEntity entityToUpdate)
        {
            if (entityToUpdate == null)
            {
                throw new InvalidOperationException("Unable to update a null entity in the repository.");
            }
            if (!typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                _dbSet.Attach(entityToUpdate);
                _context.Entry(entityToUpdate).State = EntityState.Modified;
                return entityToUpdate;
            }
            var result = await _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                .ReplaceOneAsync(t => t.Guid == entityToUpdate.Guid,
                    entityToUpdate);
            return result.IsAcknowledged ? entityToUpdate : null;
        }

        public int Count(Expression<Func<TEntity, bool>> filter = null, string includeProperties = "")
        {
            if (typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
                return
                    Convert.ToInt32(_mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                        .Count(filter));
            var query = GetGeneric(filter, includeProperties);
            return query.Count();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null, string includeProperties = "")
        {
            if (typeof(IMongoEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return Convert.ToInt32(await _mongoDb.GetCollection<TEntity>(typeof(TEntity).ToMongoCollectionName())
                        .CountAsync(filter));
            }
            var query = GetGeneric(filter, includeProperties);
            return await query.CountAsync();
        }

        private IQueryable<TEntity> GetGeneric(Expression<Func<TEntity, bool>> filter = null, string includeProperties = "", Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, bool ascending = false)
        {
            var query = filter != null ? _dbSet.Where(filter) : _dbSet;
            var navProperties = includeProperties.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if (navProperties.Length > 0)
            {
                query = navProperties.Aggregate(query, (current, nav) => current.Include(nav));
            }
            if (orderBy == null)
                return query;
            return ascending ? query.OrderByDescending(t => orderBy) : query.OrderBy(t => orderBy);
        }
    }
}
