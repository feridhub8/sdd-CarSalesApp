import { API, api } from './http';

export type User = {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string | null;
};

export type Seller = {
  firstName: string;
  phone: string | null;
};

export type Registration = {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
};

export function register(registration: Registration) {
  return api<User>(`${API}/Auth/Register`, { method: 'POST', body: registration, retryOn401: false });
}

export function login(login: string, password: string) {
  return api<{ accessToken: string }>(`${API}/Auth/Login`, { method: 'POST', body: { login, password }, retryOn401: false });
}

export function logout() {
  return api<void>(`${API}/Auth/Logout`, { method: 'POST', retryOn401: false });
}

export function getMe() {
  return api<User>(`${API}/Auth/Me`);
}

export function updatePhone(phone: string) {
  return api<User>(`${API}/Auth/Me/Phone`, { method: 'PUT', body: { phone } });
}

export function getSeller(id: string) {
  return api<Seller>(`${API}/Auth/Sellers/${encodeURIComponent(id)}`);
}
