using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Models;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface INotionTokenRepository : IRepositoryBase<NotionToken, long>;