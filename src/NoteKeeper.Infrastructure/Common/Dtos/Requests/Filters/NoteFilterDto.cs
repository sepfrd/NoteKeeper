using System.Linq.Expressions;
using System.Reflection;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Common;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Infrastructure.Common.Dtos.Requests.Filters;

public class NoteFilterDto : FilterDtoBase, IFilterBase<Note>
{
    private static readonly MethodInfo _iLikeMethod = typeof(NpgsqlDbFunctionsExtensions)
        .GetMethod(nameof(NpgsqlDbFunctionsExtensions.ILike), new[]
        {
            typeof(DbFunctions),
            typeof(string),
            typeof(string)
        })!;

    private static readonly MemberExpression _dbFunctions = Expression.Property(
        null,
        typeof(EF).GetProperty(nameof(EF.Functions))!);

    public string? Title { get; set; }

    public string? Content { get; set; }

    public Guid? UserUuid { get; set; }

    public Expression<Func<Note, bool>> ToFilterExpression()
    {
        var note = Expression.Parameter(typeof(Note), nameof(Note).Camelize());

        var expressions = new List<Expression>();

        var parentDtoExpression = ToBaseExpression(note);

        if (parentDtoExpression is not null)
        {
            expressions.Add(parentDtoExpression);
        }

        if (UserUuid is not null)
        {
            var userMember = Expression.Property(note, nameof(Note.User));

            var userUuidMember = Expression.Property(userMember, nameof(User.Uuid));

            var userUuidConstant = Expression.Constant(UserUuid);

            var userUuidExpression = Expression.Equal(userUuidMember, userUuidConstant);

            expressions.Add(userUuidExpression);
        }

        if (!string.IsNullOrWhiteSpace(Title))
        {
            var titleMember = Expression.Property(note, nameof(Note.Title));
            var titleConstant = Expression.Constant($"%{Title}%");

            var titleExpression = Expression.Call(
                _iLikeMethod,
                _dbFunctions,
                titleMember,
                titleConstant);

            expressions.Add(titleExpression);
        }

        if (!string.IsNullOrWhiteSpace(Content))
        {
            var contentMember = Expression.Property(note, nameof(Note.Content));
            var contentConstant = Expression.Constant($"%{Content}%");

            var contentExpression = Expression.Call(
                _iLikeMethod,
                _dbFunctions,
                contentMember,
                contentConstant);

            expressions.Add(contentExpression);
        }

        Expression? baseExpression = null;

        foreach (var expressionItem in expressions)
        {
            baseExpression = baseExpression switch
            {
                null => expressionItem,
                _ => Expression.AndAlso(baseExpression, expressionItem)
            };
        }

        if (baseExpression is null)
        {
            return _ => true;
        }

        var lambda = Expression.Lambda<Func<Note, bool>>(baseExpression, note);

        return lambda;
    }
}