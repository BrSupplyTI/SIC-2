using SIC.Api.Models.Auth;
using SIC.Api.Models.Profile;
using SIC.Api.Repositories;

namespace SIC.Api.Services;

public sealed class UserProfileService(IUserProfileRepository repository) : IUserProfileService
{
    public async Task<IReadOnlyList<AreaOptionDto>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        var areas = await repository.GetAreasAsync(cancellationToken);
        return areas.Select(x => new AreaOptionDto
        {
            AreaId = x.AreaId,
            Nome = x.Nome
        }).ToList();
    }

    public async Task<UserProfileDto?> GetProfileAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var profile = await repository.GetProfileAsync(usuarioId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var permissions = (await repository.GetPermissionsAsync(usuarioId, cancellationToken)).ToList();

        if (profile.FlagAdmin)
        {
            permissions.Add(new()
            {
                Modulo = "ADMINISTRADOR",
                NomePermissao = "Você é um administrador"
            });
        }

        if (profile.FlagBackOffice)
        {
            permissions.Add(new()
            {
                Modulo = "BACKOFFICE",
                NomePermissao = "Você tem acesso às funções de BackOffice"
            });
        }

        if (profile.FlagAlteraEstabelecimento)
        {
            permissions.Add(new()
            {
                Modulo = "TROCAR ESTABELECIMENTO",
                NomePermissao = "Você pode trocar de estabelecimento a qualquer momento"
            });
        }

        return new UserProfileDto
        {
            UsuarioId = profile.UsuarioId,
            Nome = profile.Nome,
            Email = profile.Email,
            Telefone = profile.Telefone,
            AreaId = profile.AreaId,
            AreaNome = profile.AreaNome,
            Foto = profile.Foto,
            Permissoes = permissions.Select(x => new UserPermissionDto
            {
                Modulo = x.Modulo,
                NomePermissao = x.NomePermissao,
                ConcedidoPor = x.ConcedidoPor,
                DataHora = x.DataHora
            }).ToList()
        };
    }

    public async Task<OperationResult> UpdateProfileAsync(int usuarioId, int? areaId, string? telefone, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "Usuário inválido."
            };
        }

        var updated = await repository.UpdateProfileAsync(usuarioId, areaId, telefone, cancellationToken);
        return new OperationResult
        {
            Success = updated,
            ErrorCode = updated ? null : "PROFILE_NOT_UPDATED",
            Message = updated ? "Dados atualizados com sucesso." : "Não foi possível atualizar os dados."
        };
    }

    public async Task<OperationResult> UpdatePhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0 || string.IsNullOrWhiteSpace(foto))
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "Foto inválida."
            };
        }

        var updated = await repository.UpdatePhotoAsync(usuarioId, foto, cancellationToken);
        return new OperationResult
        {
            Success = updated,
            ErrorCode = updated ? null : "PHOTO_NOT_UPDATED",
            Message = updated ? "Foto atualizada com sucesso." : "Não foi possível atualizar a foto."
        };
    }

    public async Task<OperationResult> RemovePhotoAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "Usuário inválido."
            };
        }

        var updated = await repository.RemovePhotoAsync(usuarioId, cancellationToken);
        return new OperationResult
        {
            Success = updated,
            ErrorCode = updated ? null : "PHOTO_NOT_REMOVED",
            Message = updated ? "Foto removida com sucesso." : "Não foi possível remover a foto."
        };
    }
}
