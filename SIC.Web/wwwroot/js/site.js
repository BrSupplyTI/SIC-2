// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(() => {
  const html = document.documentElement;
  const savedTheme = localStorage.getItem("sic-theme") || "light";
  html.setAttribute("data-bs-theme", savedTheme);

  const themeToggle = document.getElementById("themeToggle");
  if (themeToggle) {
    themeToggle.addEventListener("click", () => {
      const current = html.getAttribute("data-bs-theme") || "light";
      const next = current === "light" ? "dark" : "light";
      html.setAttribute("data-bs-theme", next);
      localStorage.setItem("sic-theme", next);

      const icon = themeToggle.querySelector("i");
      if (icon) {
        icon.className = next === "dark" ? "fa-solid fa-sun" : "fa-solid fa-moon";
      }
    });

    const icon = themeToggle.querySelector("i");
    if (icon) {
      icon.className = savedTheme === "dark" ? "fa-solid fa-sun" : "fa-solid fa-moon";
    }
  }

  const sidebarToggle = document.getElementById("sidebarToggle");
  const sidebar = document.getElementById("sicSidebar");
  if (sidebarToggle && sidebar) {
    sidebarToggle.addEventListener("click", () => {
      sidebar.classList.toggle("open");
    });
  }

  window.sicAlert = (title, text, icon = "info") => {
    if (!window.Swal) {
      alert(`${title}\n${text}`);
      return;
    }

    Swal.fire({
      title,
      text,
      icon,
      confirmButtonText: "OK"
    });
  };

  const avatar = document.getElementById("userAvatar");
  const avatarInput = document.getElementById("avatarFileInput");
  const avatarForm = document.getElementById("avatarUploadForm");

  if (avatar && avatarInput && avatarForm) {
    const openFileDialog = () => {
      avatarInput.value = "";
      avatarInput.click();
    };

    avatar.addEventListener("click", async () => {
      const hasPhoto = avatar.dataset.hasPhoto === "true";

      if (!hasPhoto) {
        openFileDialog();
        return;
      }

      if (!window.Swal) {
        if (confirm("Deseja atualizar a foto?")) {
          openFileDialog();
        }
        return;
      }

      const result = await Swal.fire({
        title: "Atualizar foto",
        text: "Deseja atualizar a foto?",
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "Sim",
        cancelButtonText: "Não"
      });

      if (result.isConfirmed) {
        openFileDialog();
      }
    });

    avatarInput.addEventListener("change", async () => {
      const file = avatarInput.files?.[0];
      if (!file) {
        return;
      }

      const allowed = ["image/jpeg", "image/png"];
      if (!allowed.includes(file.type)) {
        window.sicAlert("Formato inválido", "Selecione uma imagem JPG ou PNG.", "error");
        return;
      }

      if (file.size > 2 * 1024 * 1024) {
        window.sicAlert("Arquivo inválido", "A imagem deve ter no máximo 2MB.", "error");
        return;
      }

      const formData = new FormData();
      formData.append("avatarFile", file);

      const tokenInput = avatarForm.querySelector('input[name="__RequestVerificationToken"]');
      if (tokenInput) {
        formData.append("__RequestVerificationToken", tokenInput.value);
      }

      try {
        const response = await fetch("/Account/Avatar?handler=Upload", {
          method: "POST",
          body: formData
        });

        const result = await response.json();
        if (!result.success) {
          window.sicAlert("Ops", result.message || "Falha ao enviar a foto.", "error");
          return;
        }

        const cacheBuster = `v=${new Date().getTime()}`;
        avatar.src = `${result.imageUrl}${result.imageUrl.includes("?") ? "&" : "?"}${cacheBuster}`;
        avatar.dataset.hasPhoto = "true";
        window.sicAlert("Sucesso", "Foto atualizada com sucesso.", "success");
      } catch {
        window.sicAlert("Erro", "Não foi possível enviar a foto neste momento.", "error");
      }
    });
  }

  const changePasswordAction = document.getElementById("changePasswordAction");
  const changePasswordForm = document.getElementById("changePasswordForm");

  if (changePasswordAction && changePasswordForm) {
    changePasswordAction.addEventListener("click", async () => {
      if (!window.Swal) {
        window.sicAlert("Indisponível", "SweetAlert não está carregado.", "error");
        return;
      }

      const { value: formValues } = await Swal.fire({
        title: "Alterar senha",
        html: `
          <input id="swal-new-password" type="password" class="swal2-input" placeholder="Nova senha">
          <input id="swal-confirm-password" type="password" class="swal2-input" placeholder="Confirmar nova senha">
          <small style="display:block;color:#6b7280;padding:1rem 1.5rem;">Mínimo 7 caracteres, com letra maiúscula, número e caractere especial.</small>
        `,
        focusConfirm: false,
        showCancelButton: true,
        confirmButtonText: "Salvar",
        cancelButtonText: "Cancelar",
        preConfirm: () => {
          const newPassword = document.getElementById("swal-new-password")?.value || "";
          const confirmPassword = document.getElementById("swal-confirm-password")?.value || "";

          if (!newPassword || !confirmPassword) {
            Swal.showValidationMessage("Informe e confirme a nova senha.");
            return null;
          }

          if (newPassword !== confirmPassword) {
            Swal.showValidationMessage("A confirmação da senha não confere.");
            return null;
          }

          const rule = /^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{7,}$/;
          if (!rule.test(newPassword)) {
            Swal.showValidationMessage("A senha não atende aos critérios mínimos.");
            return null;
          }

          return { newPassword, confirmPassword };
        }
      });

      if (!formValues) {
        return;
      }

      const tokenInput = changePasswordForm.querySelector('input[name="__RequestVerificationToken"]');
      const formData = new FormData();
      formData.append("newPassword", formValues.newPassword);
      formData.append("confirmPassword", formValues.confirmPassword);
      if (tokenInput) {
        formData.append("__RequestVerificationToken", tokenInput.value);
      }

      try {
        const response = await fetch("/Account/Security?handler=ChangePassword", {
          method: "POST",
          body: formData
        });

        const result = await response.json();
        window.sicAlert(result.success ? "Sucesso" : "Ops", result.message || "Falha na alteração de senha.", result.success ? "success" : "error");
      } catch {
        window.sicAlert("Erro", "Não foi possível alterar a senha neste momento.", "error");
      }
    });
  }
})();
