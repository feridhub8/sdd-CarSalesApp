// Every text on the site lives here, so another language can be added later without touching the pages.
export const az = {
  brand: 'CarSales',
  common: {
    loading: 'Yüklənir...',
    wait: 'Gözləyin...',
    retry: 'Yenidən cəhd et',
  },
  errors: {
    unavailable: 'Xidmət hazırda əlçatan deyil. Bir az sonra yenidən cəhd edin.',
    invalid: 'Daxil etdiyiniz məlumatları yoxlayın.',
  },
  nav: {
    listings: 'Elanlar',
    myListings: 'Mənim elanlarım',
    signIn: 'Daxil ol',
    signOut: 'Çıxış',
    register: 'Qeydiyyat',
  },
  home: {
    title: 'Avtomobil elanları',
    comingSoon: 'Elanlar tezliklə burada olacaq.',
  },
  login: {
    title: 'Daxil ol',
    login: 'İstifadəçi adı və ya e-poçt',
    password: 'Şifrə',
    submit: 'Daxil ol',
    missing: 'İstifadəçi adı və ya e-poçtu və şifrəni daxil edin.',
    wrong: 'Giriş məlumatları yanlışdır.',
    registered: 'Hesabınız yaradıldı. İndi daxil ola bilərsiniz.',
    noAccount: 'Hesabınız yoxdur?',
    toRegister: 'Qeydiyyatdan keçin',
  },
  register: {
    title: 'Qeydiyyat',
    submit: 'Hesab yarat',
    taken: 'Bu məlumatlarla hesab artıq mövcuddur.',
    hasAccount: 'Artıq hesabınız var?',
    toLogin: 'Daxil olun',
    fields: {
      username: {
        label: 'İstifadəçi adı',
        invalid: 'İstifadəçi adı 3-30 simvol olmalıdır: hərf, rəqəm, nöqtə, alt xətt və ya tire.',
        taken: 'Bu istifadəçi adı artıq istifadə olunur.',
      },
      email: {
        label: 'E-poçt',
        invalid: 'Düzgün e-poçt ünvanı daxil edin.',
        taken: 'Bu e-poçt artıq qeydiyyatdan keçib.',
      },
      password: {
        label: 'Şifrə',
        hint: 'Ən azı 8 simvol: böyük hərf, kiçik hərf, rəqəm və xüsusi simvol.',
        invalid: 'Şifrə ən azı 8 simvol olmalı, böyük hərf, kiçik hərf, rəqəm və xüsusi simvol daxil etməlidir.',
      },
      firstName: {
        label: 'Ad',
        invalid: 'Adınızı daxil edin.',
      },
      lastName: {
        label: 'Soyad',
        invalid: 'Soyadınızı daxil edin.',
      },
    },
  },
  myListings: {
    title: 'Mənim elanlarım',
    count: (count: number) => `Elan sayı: ${count}`,
  },
  notFound: {
    title: 'Səhifə tapılmadı',
    text: 'Axtardığınız səhifə yoxdur və ya silinib.',
    back: 'Ana səhifəyə qayıt',
  },
  footer: 'CarSales: avtomobil alqı-satqısı',
} as const;
