import { NavLink } from 'react-router';
import { az } from '../i18n/az';

const tab = ({ isActive }: { isActive: boolean }) =>
  `rounded-lg px-4 py-2 font-medium ${isActive ? 'bg-accent text-white' : 'border border-line bg-surface hover:bg-page'}`;

export function AdminTabs() {
  return (
    <nav aria-label={az.nav.admin} className="mb-6 flex gap-2">
      <NavLink to="/admin/catalog" className={tab}>
        {az.admin.catalogTab}
      </NavLink>
      <NavLink to="/admin/listings" className={tab}>
        {az.admin.listingsTab}
      </NavLink>
    </nav>
  );
}
