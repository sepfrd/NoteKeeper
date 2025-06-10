using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Data;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface IGoogleTokenRepository : IRepositoryBase<GoogleToken, long>;