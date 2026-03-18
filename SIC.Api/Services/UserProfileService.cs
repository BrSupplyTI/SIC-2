using SIC.Api.Models.Auth;
using SIC.Api.Models.Profile;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;

    public UserProfileService(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AreaOptionDto>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        var areas = await _repository.GetAreasAsync(cancellationToken);
        return areas.Select(x => new AreaOptionDto
        {
            AreaId = x.AreaId,
            Nome = x.Nome
        }).ToList();
    }

    public async Task<UserProfileDto?> GetProfileAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetProfileAsync(usuarioId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var permissions = (await _repository.GetPermissionsAsync(usuarioId, cancellationToken)).ToList();

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
            Ramal = profile.Ramal,
            Matricula = profile.Matricula,
            Cargo = profile.Cargo,
            Setor = profile.Setor,
            AreaId = profile.AreaId,
            AreaNome = profile.AreaNome,
            Foto = profile.Foto,
            DiaAniversario = profile.DiaAniversario,
            MesAniversario = profile.MesAniversario,
            Permissoes = permissions.Select(x => new UserPermissionDto
            {
                Modulo = x.Modulo,
                NomePermissao = x.NomePermissao,
                ConcedidoPor = x.ConcedidoPor,
                DataHora = x.DataHora
            }).ToList()
        };
    }

    public async Task<OperationResult> UpdateProfileAsync(int usuarioId, int? areaId, string? telefone, string? ramal, int? matricula, string? cargo, string? setor, int? diaAniversario, int? mesAniversario, CancellationToken cancellationToken = default)
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

        if (diaAniversario.HasValue || mesAniversario.HasValue)
        {
            if (!diaAniversario.HasValue || !mesAniversario.HasValue)
            {
                return new OperationResult
                {
                    Success = false,
                    ErrorCode = "INVALID_BIRTHDAY",
                    Message = "Informe o dia e o mês do aniversário."
                };
            }

            var maxDia = mesAniversario.Value switch
            {
                2 => 29,
                4 or 6 or 9 or 11 => 30,
                _ => 31
            };

            if (mesAniversario.Value < 1 || mesAniversario.Value > 12 || diaAniversario.Value < 1 || diaAniversario.Value > maxDia)
            {
                return new OperationResult
                {
                    Success = false,
                    ErrorCode = "INVALID_BIRTHDAY",
                    Message = $"Data de aniversário inválida. O mês selecionado possui no máximo {maxDia} dias."
                };
            }
        }

        var updated = await _repository.UpdateProfileAsync(usuarioId, areaId, telefone, ramal, matricula, cargo, setor, diaAniversario, mesAniversario, cancellationToken);
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

        var updated = await _repository.UpdatePhotoAsync(usuarioId, foto, cancellationToken);
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

        var updated = await _repository.RemovePhotoAsync(usuarioId, cancellationToken);
        return new OperationResult
        {
            Success = updated,
            ErrorCode = updated ? null : "PHOTO_NOT_REMOVED",
            Message = updated ? "Foto removida com sucesso." : "Não foi possível remover a foto."
        };
    }
}
