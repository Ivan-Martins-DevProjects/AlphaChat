import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Login } from './login';
import { provideRouter } from '@angular/router';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with empty credentials', () => {
    expect(component.email).toBe('');
    expect(component.password).toBe('');
    expect(component.remember).toBe(false);
  });

  describe('emailError', () => {
    it('should return empty when not touched', () => {
      component.emailTouched = false;
      expect(component.emailError).toBe('');
    });

    it('should require email', () => {
      component.emailTouched = true;
      component.email = '';
      expect(component.emailError).toBe('Email é obrigatório');
    });

    it('should reject invalid email', () => {
      component.emailTouched = true;
      component.email = 'not-an-email';
      expect(component.emailError).toBe('Email inválido');
    });

    it('should accept valid email', () => {
      component.emailTouched = true;
      component.email = 'user@example.com';
      expect(component.emailError).toBe('');
    });
  });

  describe('passwordError', () => {
    it('should return empty when not touched', () => {
      component.passwordTouched = false;
      expect(component.passwordError).toBe('');
    });

    it('should require password', () => {
      component.passwordTouched = true;
      component.password = '';
      expect(component.passwordError).toBe('Senha é obrigatória');
    });

    it('should enforce minimum length', () => {
      component.passwordTouched = true;
      component.password = '12345';
      expect(component.passwordError).toBe('Senha deve ter no mínimo 6 caracteres');
    });

    it('should accept valid password', () => {
      component.passwordTouched = true;
      component.password = '123456';
      expect(component.passwordError).toBe('');
    });
  });

  describe('isFormValid', () => {
    it('should be false with empty fields', () => {
      expect(component.isFormValid).toBe(false);
    });

    it('should be true with valid credentials', () => {
      component.emailTouched = true;
      component.passwordTouched = true;
      component.email = 'user@example.com';
      component.password = '123456';
      expect(component.isFormValid).toBe(true);
    });
  });

  it('should toggle modal visibility on closeModal', () => {
    component.showModal = true;
    component.closeModal();
    expect(component.showModal).toBe(false);
  });
});
