using System.Linq.Expressions;
using Humanizer;
using NoteKeeper.Application.Common;
using NoteKeeper.Domain.Entities;

namespace NoteKeeper.Application.Features.Notes.Dtos;

public class NoteFilterDto : FilterDtoBase
{
    public string? Title { get; set; }

    public string? Content { get; set; }

    public long? UserId { get; set; }

    public Expression<Func<Note, bool>>? ToExpression()
    {
        var note = Expression.Parameter(typeof(Note), nameof(Note).Camelize());

        var expressions = new List<Expression>();

        var parentDtoExpression = ToBaseExpression(note);

        if (parentDtoExpression is not null)
        {
            expressions.Add(parentDtoExpression);
        }

        if (UserId is not null)
        {
            var authorIdMember = Expression.Property(note, nameof(Note.UserId));
            var authorIdConstant = Expression.Constant(UserId);

            var authorIdExpression = Expression.Equal(authorIdMember, authorIdConstant);

            expressions.Add(authorIdExpression);
        }

        if (!string.IsNullOrWhiteSpace(Title))
        {
            var titleMember = Expression.Property(note, nameof(Note.Title));
            var titleConstant = Expression.Constant(Title);

            var containsMethod = typeof(string)
                .GetMethods()
                .First(methodInfo => methodInfo.Name == nameof(string.Contains) && methodInfo.GetParameters().Length == 1);

            var titleExpression = Expression.Call(titleMember, containsMethod, titleConstant);

            expressions.Add(titleExpression);
        }

        if (!string.IsNullOrWhiteSpace(Content))
        {
            var contentMember = Expression.Property(note, nameof(Note.Content));
            var contentConstant = Expression.Constant(Content);

            var containsMethod = typeof(string)
                .GetMethods()
                .First(methodInfo => methodInfo.Name == nameof(string.Contains) && methodInfo.GetParameters().Length == 1);

            var contentExpression = Expression.Call(contentMember, containsMethod, contentConstant);

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
            return null;
        }

        var lambda = Expression.Lambda<Func<Note, bool>>(baseExpression, note);

        return lambda;
    }
}