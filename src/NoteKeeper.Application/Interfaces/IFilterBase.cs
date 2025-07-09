using System.Linq.Expressions;

namespace NoteKeeper.Application.Interfaces;

public interface IFilterBase<TEntity>
{
    Expression<Func<TEntity, bool>> ToFilterExpression();
}