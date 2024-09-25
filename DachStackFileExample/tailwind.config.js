module.exports = {
    content: [
      './wwwroot/**/*.{html,js}',
      './public/index.html'
    ],
    theme: {
      extend: {},
    },
    plugins: [
      require('daisyui'),
    ],
    safelist: [
      'avatar',
      'w-48',
      'h-48',
      'w-24',
      'h-24',
      'rounded-full',
      'text-center',
    ],
  }
  