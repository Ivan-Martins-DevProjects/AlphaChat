import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Modal } from '../../components/modal/modal';
import { ApiResponse } from '../../models/api-response';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, Modal],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  email = '';
  password = '';
  remember = false;
  isLoading = false;

  showModal = false;
  modalTitle = 'Erro';
  modalMessage = '';
  modalType: 'error' | 'success' | 'warning' = 'error';

  emailTouched = false;
  passwordTouched = false;

  constructor(private router: Router, private cdr: ChangeDetectorRef) {}

  closeModal() {
    this.showModal = false;
    this.cdr.detectChanges();
  }

  get emailError(): string {
    if (!this.emailTouched) return '';
    if (!this.email) return 'Email é obrigatório';
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email)) return 'Email inválido';
    return '';
  }

  get passwordError(): string {
    if (!this.passwordTouched) return '';
    if (!this.password) return 'Senha é obrigatória';
    if (this.password.length < 6) return 'Senha deve ter no mínimo 6 caracteres';
    return '';
  }

  get isFormValid(): boolean {
    return !this.emailError && !this.passwordError && !!this.email && !!this.password;
  }

  onEmailBlur() {
    this.emailTouched = true;
    this.cdr.detectChanges();
  }

  onPasswordBlur() {
    this.passwordTouched = true;
    this.cdr.detectChanges();
  }

  async onSubmit(event: Event) {
    event.preventDefault();

    this.emailTouched = true;
    this.passwordTouched = true;
    this.cdr.detectChanges();

    if (!this.isFormValid) return;

    this.isLoading = true;

    try {
      const form = event.target as HTMLFormElement
      const formData = new FormData(form)
      const email = formData.get('email')
      const password = formData.get('password')

      const response = await fetch(`${environment.apiUrl}/api/login`, {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
      })

      if (response.status != 200) {
        const errorText = await response.text();

        try {
          const error: ApiResponse = JSON.parse(errorText);
          this.modalMessage = error.message || 'Erro ao realizar login';
        } catch (e) {
          this.modalMessage = errorText || 'Erro ao realizar login';
        }

        this.modalType = 'error';
        this.showModal = true;
        this.cdr.detectChanges();
      } else {
        this.router.navigate(['/home'])
      }
    } catch (err) {
      this.modalMessage = 'Erro ao conectar com o servidor';
      this.modalType = 'error';
      this.showModal = true;
      this.cdr.detectChanges();
    } finally {
      this.isLoading = false;
    }
  }
}
