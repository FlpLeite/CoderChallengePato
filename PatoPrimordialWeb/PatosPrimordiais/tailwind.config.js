/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        night: {
          950: "#070815",
          900: "#0b1220",
          800: "#101a2b",
          700: "#17253a",
        }
      }
    },
  },
  plugins: [],
}
