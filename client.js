// ═══════════════════════════════════════════════════════
//  طبقة الاتصال بالباكند — التوكن بيتحط تلقائياً
// ═══════════════════════════════════════════════════════

const TOKEN_KEY = 'masareef_token';

export const getToken = () => localStorage.getItem(TOKEN_KEY);
export const setToken = (t) => localStorage.setItem(TOKEN_KEY, t);
export const clearToken = () => localStorage.removeItem(TOKEN_KEY);

// بيحوّل كائن الفلتر لـ query string وبيتجاهل الفاضي
function qs(obj = {}) {
  const p = new URLSearchParams();
  Object.entries(obj).forEach(([k, v]) => {
    if (v !== null && v !== undefined && v !== '') p.set(k, v);
  });
  const s = p.toString();
  return s ? `?${s}` : '';
}

async function request(path, { method = 'GET', body, auth = true } = {}) {
  const headers = { 'Content-Type': 'application/json' };

  if (auth) {
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`/api${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  if (res.status === 401) {
    clearToken();
    window.location.hash = '#/login';
    throw new Error('انتهت الجلسة، سجّل الدخول من جديد.');
  }

  if (res.status === 204) return null;

  let data = null;
  const text = await res.text();
  if (text) {
    try { data = JSON.parse(text); } catch { data = text; }
  }

  if (!res.ok) {
    // رسالة أوضح بدل "خطأ غير متوقّع" العامة
    let msg = data?.error || data?.message;

    if (!msg) {
      if (res.status === 404) msg = `المسار غير موجود (${path}) — تأكد إن الـ endpoint موجود في الباكند.`;
      else if (res.status === 400) msg = 'بيانات الطلب غير صحيحة.';
      else if (res.status === 422) msg = 'البيانات مرفوضة — راجع القيم المدخلة.';
      else if (res.status >= 500) msg = 'خطأ في السيرفر — راجع نافذة الباكند.';
      else msg = `حدث خطأ (${res.status}).`;
    }

    throw new Error(msg);
  }

  return data;
}

// ─────────── المصادقة ───────────
export const authApi = {
  login: (email, password) =>
    request('/auth/login', { method: 'POST', body: { email, password }, auth: false }),
  register: (dto) =>
    request('/auth/register', { method: 'POST', body: dto, auth: false }),
};

// ─────────── الديون ───────────
export const debtsApi = {
  getAll: (status) => request(`/debts${qs({ status })}`),
  getById: (id) => request(`/debts/${id}`),
  getOverdue: () => request('/debts/overdue'),
  getPending: () => request('/debts/pending'),
  getIOwe: () => request('/debts/i-owe'),
  getOwedToMe: () => request('/debts/owed-to-me'),
  getTotals: () => request('/debts/summary/totals'),

  advancedSearch: (filter) => request(`/debts/search/advanced${qs(filter)}`),

  create: (dto) => request('/debts', { method: 'POST', body: dto }),
  update: (id, dto) => request(`/debts/${id}`, { method: 'PUT', body: dto }),
  addPayment: (id, dto) => request(`/debts/${id}/payments`, { method: 'POST', body: dto }),
  writeOff: (id, reason) => request(`/debts/${id}/write-off`, { method: 'POST', body: { reason } }),
};

// ─────────── البيت ───────────
export const homeApi = {
  getExpenses: (year, month, categoryId) =>
    request(`/home/expenses${qs({ year, month, categoryId })}`),
  getIncomes: (year, month) => request(`/home/incomes${qs({ year, month })}`),

  advancedSearchExpenses: (filter) =>
    request(`/home/expenses/search/advanced${qs(filter)}`),
  advancedSearchIncomes: (filter) =>
    request(`/home/incomes/search/advanced${qs(filter)}`),

  addExpense: (dto) => request('/home/expenses', { method: 'POST', body: dto }),
  addIncome: (dto) => request('/home/incomes', { method: 'POST', body: dto }),
  updateExpense: (id, dto) => request(`/home/expenses/${id}`, { method: 'PUT', body: dto }),
  updateIncome: (id, dto) => request(`/home/incomes/${id}`, { method: 'PUT', body: dto }),
  deleteExpense: (id) => request(`/home/expenses/${id}`, { method: 'DELETE' }),
  deleteIncome: (id) => request(`/home/incomes/${id}`, { method: 'DELETE' }),
};

// ─────────── البيزنس ───────────
export const businessApi = {
  getAll: () => request('/businesses'),
  getById: (id) => request(`/businesses/${id}`),
  getInactive: () => request('/businesses/inactive'),
  advancedSearch: (filter) => request(`/businesses/search/advanced${qs(filter)}`),

  getProfits: (year, month) => request(`/businesses/profits${qs({ year, month })}`),
  getProfit: (id, year, month) => request(`/businesses/${id}/profit${qs({ year, month })}`),

  create: (dto) => request('/businesses', { method: 'POST', body: dto }),
  update: (id, dto) => request(`/businesses/${id}`, { method: 'PUT', body: dto }),
  deactivate: (id) => request(`/businesses/${id}`, { method: 'DELETE' }),
  reactivate: (id) => request(`/businesses/${id}/reactivate`, { method: 'POST' }),

  getExpenses: (id, year, month) => request(`/businesses/${id}/expenses${qs({ year, month })}`),
  getIncomes: (id, year, month) => request(`/businesses/${id}/incomes${qs({ year, month })}`),
  searchExpenses: (id, filter) => request(`/businesses/${id}/expenses/search${qs(filter)}`),
  searchIncomes: (id, filter) => request(`/businesses/${id}/incomes/search${qs(filter)}`),

  addExpense: (id, dto) => request(`/businesses/${id}/expenses`, { method: 'POST', body: dto }),
  addIncome: (id, dto) => request(`/businesses/${id}/incomes`, { method: 'POST', body: dto }),
  updateExpense: (expenseId, dto) =>
    request(`/businesses/expenses/${expenseId}`, { method: 'PUT', body: dto }),
  updateIncome: (incomeId, dto) =>
    request(`/businesses/incomes/${incomeId}`, { method: 'PUT', body: dto }),
  deleteExpense: (expenseId) =>
    request(`/businesses/expenses/${expenseId}`, { method: 'DELETE' }),
  deleteIncome: (incomeId) =>
    request(`/businesses/incomes/${incomeId}`, { method: 'DELETE' }),
};

// ─────────── المنتجات والمبيعات ───────────
export const productsApi = {
  // منتجات
  getByBusiness: (bizId) => request(`/businesses/${bizId}/products`),
  getById: (id) => request(`/products/${id}`),
  search: (bizId, filter) => request(`/businesses/${bizId}/products/search${qs(filter)}`),
  searchByName: (bizId, name) => request(`/businesses/${bizId}/products/search/by-name${qs({ name })}`),

  // قوائم جاهزة
  getOutOfStock: (bizId) => request(`/businesses/${bizId}/products/out-of-stock`),
  getLowStock: (bizId, threshold = 5) =>
    request(`/businesses/${bizId}/products/low-stock${qs({ threshold })}`),
  getNeverSold: (bizId) => request(`/businesses/${bizId}/products/never-sold`),
  getBestSellers: (bizId) => request(`/businesses/${bizId}/products/best-sellers`),
  getInactive: (bizId) => request(`/businesses/${bizId}/products/inactive`),

  // كتابة المنتج
  create: (bizId, dto) => request(`/businesses/${bizId}/products`, { method: 'POST', body: dto }),
  update: (id, dto) => request(`/products/${id}`, { method: 'PUT', body: dto }),
  deactivate: (id) => request(`/products/${id}`, { method: 'DELETE' }),
  reactivate: (id) => request(`/products/${id}/reactivate`, { method: 'POST' }),

  // مبيعات
  getSales: (bizId, filter) => request(`/businesses/${bizId}/sales${qs(filter)}`),
  getProductSales: (bizId, productId) =>
    request(`/businesses/${bizId}/products/${productId}/sales`),
  addSale: (productId, dto) => request(`/products/${productId}/sales`, { method: 'POST', body: dto }),
  updateSale: (saleId, dto) => request(`/sales/${saleId}`, { method: 'PUT', body: dto }),
  deleteSale: (saleId) => request(`/sales/${saleId}`, { method: 'DELETE' }),
};

// ─────────── التقارير ───────────
export const reportsApi = {
  dashboard: (year, month) => request(`/reports/dashboard${qs({ year, month })}`),
  homeSpending: (year, month) => request(`/reports/home-spending${qs({ year, month })}`),
  homeBreakdown: (year, month) => request(`/reports/home-breakdown${qs({ year, month })}`),
  homeTrend: (months = 6) => request(`/reports/home-trend${qs({ months })}`),
};

// ─────────── التصنيفات ───────────
export const categoriesApi = {
  get: (scope, kind) => request(`/categories${qs({ scope, kind })}`),
  getHomeExpenses: () => request('/categories/home/expenses'),
  getHomeIncomes: () => request('/categories/home/incomes'),
  getBusinessExpenses: () => request('/categories/business/expenses'),
  getBusinessIncomes: () => request('/categories/business/incomes'),

  create: (dto) => request('/categories', { method: 'POST', body: dto }),
  update: (id, dto) => request(`/categories/${id}`, { method: 'PUT', body: dto }),
  delete: (id) => request(`/categories/${id}`, { method: 'DELETE' }),
};
