using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Infrastructure.ExternalServices.Notion.Data;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface INotionTokenRepository : IRepositoryBase<NotionToken, long>;