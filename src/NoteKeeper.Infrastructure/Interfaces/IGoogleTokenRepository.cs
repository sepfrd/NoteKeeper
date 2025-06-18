using NoteKeeper.Application.Interfaces.Repositories;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface IGoogleTokenRepository : IRepositoryBase<GoogleToken, long>;