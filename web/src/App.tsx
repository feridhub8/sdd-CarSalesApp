import { Route, Routes } from 'react-router';
import { RequireAdmin, RequireAuth } from './auth/RequireAuth';
import { Layout } from './components/Layout';
import { AdminCatalogPage } from './pages/AdminCatalogPage';
import { AdminListingsPage } from './pages/AdminListingsPage';
import { HomePage } from './pages/HomePage';
import { ListingFormPage } from './pages/ListingFormPage';
import { ListingImagesPage } from './pages/ListingImagesPage';
import { ListingPage } from './pages/ListingPage';
import { LoginPage } from './pages/LoginPage';
import { MyListingsPage } from './pages/MyListingsPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { RegisterPage } from './pages/RegisterPage';

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<HomePage />} />
        <Route path="listings/:id" element={<ListingPage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route element={<RequireAuth />}>
          <Route path="my-listings" element={<MyListingsPage />} />
          <Route path="listings/new" element={<ListingFormPage />} />
          <Route path="listings/:id/edit" element={<ListingFormPage />} />
          <Route path="listings/:id/images" element={<ListingImagesPage />} />
          <Route element={<RequireAdmin />}>
            <Route path="admin/catalog" element={<AdminCatalogPage />} />
            <Route path="admin/listings" element={<AdminListingsPage />} />
          </Route>
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
