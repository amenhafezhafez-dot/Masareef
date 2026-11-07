import { createContext, useContext, useState, useCallback } from 'react';
import { authApi, setToken, clearToken, getToken } from '../api/client';

const AuthContext = createContext(null);

// بنقرا بيانات المستخدم المخزّنة عشان ميتسجّلش خروج مع كل refresh
function readStoredUser() {
  try {
    const raw = localStorage.getItem('masareef_user');
    return raw ? JSON.parse(raw) : null;
  } catch { return null; }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => (getToken() ? readStoredUser() : null));

  const persist = (result) => {
    setToken(result.token);
    const u = { userId: result.userId, name: result.name, email: result.email };
    localStorage.setItem('masareef_user', JSON.stringify(u));
    setUser(u);
  };

  const login = useCallback(async (email, password) => {
    const result = await authApi.login(email, password);
    persist(result);
    return result;
  }, []);

  const register = useCallback(async (dto) => {
    const result = await authApi.register(dto);
    persist(result);
    return result;
  }, []);

  const logout = useCallback(() => {
    clearToken();
    localStorage.removeItem('masareef_user');
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, login, register, logout, isAuthed: !!user }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
