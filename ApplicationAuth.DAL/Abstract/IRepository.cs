using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ApplicationAuth.DAL.Abstract
{
    public interface IRepository<T> : IDisposable where T : class
    {
        /// <summary>
        /// Gets entities as an IQueryable<T> table.
        /// </summary>
        IQueryable<T> Table { get; }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>A collection of entities.</returns>
        IList<T> GetAll();

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A collection that contains elements from the input sequence that satisfy the condition specified by predicate.</returns>
        IQueryable<T> Get(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>Default value if source is empty or if no element passes the test specified by predicate;
        /// otherwise, the first element in source that passes the test specified by predicate.</returns>
        T Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Returns the element of a sequence whose Id satisfies a specified condition or a default value if no such element is found.
        /// </summary>
        /// <param name="id">A specified condition of Id.</param>
        /// <returns>Default value if source is empty or if no element passes the test specified by condition of Id;
        /// otherwise, the element in source that passes the test specified by condition of Id.</returns>
        T GetById(object id);

        /// <summary>
        /// Begins tracking the given entity in the Added state such that they will be inserted into the database.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Insert(T entity);

        /// <summary>
        /// Asynchronously begins tracking the given entity in the Added state such that they will be inserted into the database.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        Task InsertAsync(T entity);

        /// <summary>
        /// Begins tracking the given entity and entries reachable from the given entity using the Modified state such that it will be modified from the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(T entity);

        /// <summary>
        /// Asynchronously begins tracking the given entity and entries reachable from the given entity using the Modified state such that it will be modified from the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Begins tracking the given entity in the Deleted state such that it will be removed from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(T entity);

        /// <summary>
        /// Begins tracking the entity with given Id in the Deleted state such that it will be removed from the database.
        /// </summary>
        /// <param name="id">The id of an entity to delete.</param>
        void DeleteById(int id);

        /// <summary>
        /// Creates a LINQ query based on a raw SQL query.
        /// </summary>
        /// <param name="storedProcedure">The command to execute.</param>
        /// <param name="parameters">The values to be assigned to parameters.</param>
        /// <returns>An IEnumerable<S> representing the raw SQL query.</returns>
        IEnumerable<S> ExecuteStoredProcedure<S>(string storedProcedure, Dictionary<string, object> parameters) where S : class;

        /// <summary>
        /// Asynchronously determines whether a sequence contains any elements or any element of a sequence satisfies a condition.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true
        /// if the source sequence contains any elements or any elements in the source sequence pass the test in the specified predicate;
        /// otherwise, false.</returns>
        Task<bool> Any(Expression<Func<T, bool>> predicate = null);
    }
}
