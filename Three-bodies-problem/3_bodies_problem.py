import pygame
import math
import sys

# Inicjalizacja pygame
pygame.init()

# Stałe
WIDTH, HEIGHT = 1600, 1000
PANEL_WIDTH = 300
SIM_WIDTH = WIDTH - PANEL_WIDTH
G = 6.67430e-11  # Stała grawitacyjna (skalowana dla wizualizacji)
SCALE = 1e9  # Skala do konwersji metrów na piksele
DT_BASE = 1000  # Bazowy krok czasowy w sekundach

# Kolory
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)
GRAY = (200, 200, 200)
DARK_GRAY = (100, 100, 100)
BLUE = (100, 150, 255)
RED = (255, 100, 100)
GREEN = (100, 255, 100)
YELLOW = (255, 255, 100)
ORANGE = (255, 165, 0)

COLORS = [RED, GREEN, BLUE, YELLOW, ORANGE]

class Body:
    def __init__(self, x, y, vx, vy, mass, color):
        self.x = x
        self.y = y
        self.vx = vx
        self.vy = vy
        self.mass = mass
        self.color = color
        self.trail = []
        self.max_trail = 500
        self.current_force_x = 0.0  # Składowa X siły
        self.current_force_y = 0.0  # Składowa Y siły
        self.current_force = 0.0    # Siła wypadkowa
        
    def get_radius(self):
        # Masa Ziemi = 5.972 × 10^24 kg
        # Promień Ziemi jako odniesienie = 10 pikseli
        earth_mass = 5.972e24
        earth_radius_pixels = 10
        
        # Promień proporcjonalny do rzeczywistego promienia (R ~ M^(1/3) dla stałej gęstości)
        # Ale używamy bardziej subtelnej zależności dla lepszej wizualizacji
        radius = earth_radius_pixels * (self.mass / earth_mass) ** 0.35
        return max(4, min(40, radius))  # Ograniczenie 4-40 pikseli
    
    def update_position(self, dt, walls_enabled=False):
        self.x += self.vx * dt
        self.y += self.vy * dt
        
        # Odbijanie od ścian jeśli włączone
        if walls_enabled:
            # Granice w metrach (obszar symulacji)
            max_x = (SIM_WIDTH / 2) * SCALE
            max_y = (HEIGHT / 2) * SCALE
            
            # Odbicie od lewej/prawej ściany
            if self.x < -max_x:
                self.x = -max_x
                self.vx = abs(self.vx) * 0.95  # Odbicie z małą stratą energii
            elif self.x > max_x:
                self.x = max_x
                self.vx = -abs(self.vx) * 0.95
            
            # Odbicie od górnej/dolnej ściany
            if self.y < -max_y:
                self.y = -max_y
                self.vy = abs(self.vy) * 0.95
            elif self.y > max_y:
                self.y = max_y
                self.vy = -abs(self.vy) * 0.95
        
        # Dodaj do śladu
        screen_x = int(self.x / SCALE + SIM_WIDTH / 2)
        screen_y = int(HEIGHT / 2 - self.y / SCALE)
        self.trail.append((screen_x, screen_y))
        
        if len(self.trail) > self.max_trail:
            self.trail.pop(0)
    
    def apply_force(self, fx, fy, dt):
        """Aplikuje siłę do ciała"""
        ax = fx / self.mass
        ay = fy / self.mass
        self.vx += ax * dt
        self.vy += ay * dt
    
    def distance_to(self, other):
        """Oblicza odległość do innego ciała"""
        dx = other.x - self.x
        dy = other.y - self.y
        return math.sqrt(dx**2 + dy**2)
    
    def draw(self, screen):
        """Rysuje ciało i jego ślad"""
        # Rysuj ślad
        if len(self.trail) > 1:
            pygame.draw.lines(screen, self.color, False, self.trail, 1)
        
        # Rysuj ciało
        screen_x = int(self.x / SCALE + SIM_WIDTH / 2)
        screen_y = int(HEIGHT / 2 - self.y / SCALE)
        radius = int(self.get_radius())
        pygame.draw.circle(screen, self.color, (screen_x, screen_y), radius)
        pygame.draw.circle(screen, WHITE, (screen_x, screen_y), radius, 1)


class InputBox:
    def __init__(self, x, y, w, h, label, default_text=''):
        self.rect = pygame.Rect(x, y, w, h)
        self.color = GRAY
        self.text = default_text
        self.label = label
        self.active = False
        self.font = pygame.font.Font(None, 24)
        
    def handle_event(self, event):
        if event.type == pygame.MOUSEBUTTONDOWN:
            if self.rect.collidepoint(event.pos):
                self.active = True
                self.color = BLUE
            else:
                self.active = False
                self.color = GRAY
                
        if event.type == pygame.KEYDOWN and self.active:
            if event.key == pygame.K_RETURN:
                self.active = False
                self.color = GRAY
            elif event.key == pygame.K_BACKSPACE:
                self.text = self.text[:-1]
            else:
                if len(self.text) < 15:
                    self.text += event.unicode
    
    def draw(self, screen):
        # Rysuj label
        label_surface = self.font.render(self.label, True, WHITE)
        screen.blit(label_surface, (self.rect.x, self.rect.y - 25))
        
        # Rysuj box
        pygame.draw.rect(screen, self.color, self.rect, 2)
        text_surface = self.font.render(self.text, True, WHITE)
        screen.blit(text_surface, (self.rect.x + 5, self.rect.y + 5))
    
    def get_value(self):
        try:
            return float(self.text) if self.text else 0.0
        except:
            return 0.0


class Button:
    def __init__(self, x, y, w, h, text, color=GRAY):
        self.rect = pygame.Rect(x, y, w, h)
        self.text = text
        self.color = color
        self.font = pygame.font.Font(None, 28)
        
    def draw(self, screen):
        pygame.draw.rect(screen, self.color, self.rect)
        pygame.draw.rect(screen, WHITE, self.rect, 2)
        text_surface = self.font.render(self.text, True, BLACK)
        text_rect = text_surface.get_rect(center=self.rect.center)
        screen.blit(text_surface, text_rect)
    
    def is_clicked(self, pos):
        return self.rect.collidepoint(pos)


class Simulator:
    def __init__(self):
        self.screen = pygame.display.set_mode((WIDTH, HEIGHT))
        pygame.display.set_caption("Symulator Problemu 3 Ciał")
        self.clock = pygame.time.Clock()
        self.running = True
        self.simulating = False
        
        self.bodies = []
        self.speed = 1.0
        self.merge_enabled = True  # Włączone łączenie ciał
        self.walls_enabled = False  # Wyłączone ściany (odbijanie)
        
        # Tworzenie input boxów dla trzech ciał
        self.input_boxes = []
        start_y = 60
        spacing = 60
        
        for i in range(3):
            y_offset = i * 220
            
            # Para x, y w jednym rzędzie
            self.input_boxes.append(InputBox(SIM_WIDTH + 20, start_y + y_offset, 120, 30, "x (km)", "0"))
            self.input_boxes.append(InputBox(SIM_WIDTH + 160, start_y + y_offset, 120, 30, "y (km)", "0"))
            
            # Para Vx, Vy w drugim rzędzie
            self.input_boxes.append(InputBox(SIM_WIDTH + 20, start_y + spacing + y_offset, 120, 30, "Vx (m/s)", "0"))
            self.input_boxes.append(InputBox(SIM_WIDTH + 160, start_y + spacing + y_offset, 120, 30, "Vy (m/s)", "0"))
            
            # Masa w trzecim rzędzie
            self.input_boxes.append(InputBox(SIM_WIDTH + 20, start_y + 2*spacing + y_offset, 260, 30, "Masa (10²⁴ kg)", "1"))
        
        # Przyciski - przesunięte niżej aby nie nachodzić na pola
        button_start_y = 820
        self.start_button = Button(SIM_WIDTH + 20, button_start_y, 125, 40, "START", GREEN)
        self.pause_button = Button(SIM_WIDTH + 155, button_start_y, 125, 40, "PAUZA", YELLOW)
        self.reset_button = Button(SIM_WIDTH + 20, button_start_y + 50, 260, 40, "RESET", RED)
        
        # Checkboxy dla opcji 
        checkbox_start_y = button_start_y - 40
        self.merge_checkbox_rect = pygame.Rect(SIM_WIDTH + 20, checkbox_start_y, 20, 20)
        self.walls_checkbox_rect = pygame.Rect(SIM_WIDTH + 20, checkbox_start_y - 25, 20, 20)
        
        # Slider prędkości - nad przyciskami
        self.speed_slider_rect = pygame.Rect(SIM_WIDTH + 20, 700, 260, 10)
        self.speed_slider_handle = pygame.Rect(SIM_WIDTH + 20 + 130, 695, 10, 20)
        self.dragging_slider = False
        
        self.font = pygame.font.Font(None, 28)
        self.small_font = pygame.font.Font(None, 20)
        
        # Ustaw domyślne wartości dla przykładowej konfiguracji
        self.set_default_values()
    
    def set_default_values(self):
        # Ciało 1 - lewa strona
        self.input_boxes[0].text = "-250000"  # x
        self.input_boxes[1].text = "-250000"     # y
        self.input_boxes[2].text = "10000"     # vx
        self.input_boxes[3].text = "0"  # vy
        self.input_boxes[4].text = "3"     # masa
        
        # Ciało 2 - prawa strona zielone
        self.input_boxes[5].text = "150000"   # x
        self.input_boxes[6].text = "150000"     # y
        self.input_boxes[7].text = "-50000"     # vx
        self.input_boxes[8].text = "0" # vy
        self.input_boxes[9].text = "2"     # masa
        
        # Ciało 3 - góra niebieskie
        self.input_boxes[10].text = "0"    # x
        self.input_boxes[11].text = "0"  # y
        self.input_boxes[12].text = "10000" # vx
        self.input_boxes[13].text = "0"    # vy
        self.input_boxes[14].text = "1"    # masa
    
    def clear_inputs(self):
        for box in self.input_boxes:
            box.text = "0"
    
    def create_bodies(self):
        self.bodies = []
        for i in range(3):
            idx = i * 5
            x = self.input_boxes[idx].get_value() * 1e6  # km na metry
            y = self.input_boxes[idx + 1].get_value() * 1e6
            vx = self.input_boxes[idx + 2].get_value()
            vy = self.input_boxes[idx + 3].get_value()
            mass = self.input_boxes[idx + 4].get_value() * 1e30  # 10^24 kg
            
            if mass > 0:  # Tylko jeśli masa jest większa od 0
                body = Body(x, y, vx, vy, mass, COLORS[i])
                self.bodies.append(body)
    
    def calculate_forces(self):
        forces = [(0, 0) for _ in self.bodies]
        
        for i, body1 in enumerate(self.bodies):
            for j, body2 in enumerate(self.bodies):
                if i != j:
                    dx = body2.x - body1.x
                    dy = body2.y - body1.y
                    distance = math.sqrt(dx**2 + dy**2)
                    
                    # Unikaj dzielenia przez zero
                    if distance < 1e6:  # 1000 km
                        distance = 1e6
                    
                    # Siła grawitacyjna
                    force = G * body1.mass * body2.mass / (distance ** 2)
                    
                    # Składowe siły
                    fx = force * dx / distance
                    fy = force * dy / distance
                    
                    forces[i] = (forces[i][0] + fx, forces[i][1] + fy)
        
        # Zapisz składowe i wielkość siły dla każdego ciała
        for i, body in enumerate(self.bodies):
            body.current_force_x = forces[i][0]
            body.current_force_y = forces[i][1]
            body.current_force = math.sqrt(forces[i][0]**2 + forces[i][1]**2)
        
        return forces
    
    def merge_bodies(self):
        merged = True
        while merged:
            merged = False
            for i in range(len(self.bodies)):
                for j in range(i + 1, len(self.bodies)):
                    if i < len(self.bodies) and j < len(self.bodies):
                        distance = self.bodies[i].distance_to(self.bodies[j])
                        
                        # Jeśli odległość jest mniejsza niż suma promieni
                        min_distance = (self.bodies[i].get_radius() + self.bodies[j].get_radius()) * SCALE
                        
                        if distance < min_distance:  # Próg łączenia
                            # Zachowanie pędu i masy
                            total_mass = self.bodies[i].mass + self.bodies[j].mass
                            
                            new_vx = (self.bodies[i].vx * self.bodies[i].mass + 
                                     self.bodies[j].vx * self.bodies[j].mass) / total_mass
                            new_vy = (self.bodies[i].vy * self.bodies[i].mass + 
                                     self.bodies[j].vy * self.bodies[j].mass) / total_mass
                            
                            new_x = (self.bodies[i].x * self.bodies[i].mass + 
                                    self.bodies[j].x * self.bodies[j].mass) / total_mass
                            new_y = (self.bodies[i].y * self.bodies[i].mass + 
                                    self.bodies[j].y * self.bodies[j].mass) / total_mass
                            
                            # Mieszaj kolory
                            new_color = tuple((c1 + c2) // 2 for c1, c2 in 
                                            zip(self.bodies[i].color, self.bodies[j].color))
                            
                            # Utwórz nowe ciało
                            new_body = Body(new_x, new_y, new_vx, new_vy, total_mass, new_color)
                            
                            # Usuń stare ciała i dodaj nowe
                            if j < len(self.bodies):
                                self.bodies.pop(j)
                            if i < len(self.bodies):
                                self.bodies.pop(i)
                            self.bodies.append(new_body)
                            
                            merged = True
                            break
                if merged:
                    break
    
    def update_simulation(self):
        if not self.simulating or len(self.bodies) == 0:
            return
        
        dt = DT_BASE * self.speed
        
        # Oblicz siły
        forces = self.calculate_forces()
        
        # Aplikuj siły i aktualizuj pozycje
        for i, body in enumerate(self.bodies):
            body.apply_force(forces[i][0], forces[i][1], dt)
            body.update_position(dt, self.walls_enabled)
        
        # Sprawdź czy ciała się zderzają (tylko jeśli włączone)
        if self.merge_enabled:
            self.merge_bodies()
    
    def handle_events(self):
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
            
            # Pauza/wznowienie spacją
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_SPACE and len(self.bodies) > 0:
                    self.simulating = not self.simulating
            
            # Obsługa input boxów
            for box in self.input_boxes:
                box.handle_event(event)
            
            # Obsługa przycisków
            if event.type == pygame.MOUSEBUTTONDOWN:
                pos = event.pos
                
                if self.start_button.is_clicked(pos):
                    self.create_bodies()
                    self.simulating = True
                
                elif self.pause_button.is_clicked(pos):
                    if len(self.bodies) > 0:
                        self.simulating = not self.simulating
                    
                elif self.reset_button.is_clicked(pos):
                    self.simulating = False
                    self.bodies = []
                    self.clear_inputs()
                    self.set_default_values()
                
                # Obsługa checkboxów (tylko gdy symulacja nie działa)
                elif not self.simulating:
                    if self.merge_checkbox_rect.collidepoint(pos):
                        self.merge_enabled = not self.merge_enabled
                    elif self.walls_checkbox_rect.collidepoint(pos):
                        self.walls_enabled = not self.walls_enabled
                
                # Slider
                if self.speed_slider_handle.collidepoint(event.pos):
                    self.dragging_slider = True
            
            if event.type == pygame.MOUSEBUTTONUP:
                self.dragging_slider = False
            
            if event.type == pygame.MOUSEMOTION and self.dragging_slider:
                # Aktualizuj pozycję slidera
                new_x = max(self.speed_slider_rect.x, 
                           min(event.pos[0], self.speed_slider_rect.x + self.speed_slider_rect.width))
                self.speed_slider_handle.x = new_x
                
                # Oblicz prędkość (0.1 do 100)
                ratio = (new_x - self.speed_slider_rect.x) / self.speed_slider_rect.width
                self.speed = 0.01 + ratio * 199.99
    
    def draw(self):
        self.screen.fill(BLACK)
        
        # Rysuj obszar symulacji z ramką jeśli włączona
        pygame.draw.rect(self.screen, DARK_GRAY, (0, 0, SIM_WIDTH, HEIGHT))
        
        # Rysuj ramkę ścian jeśli włączona
        if self.walls_enabled:
            pygame.draw.rect(self.screen, YELLOW, (2, 2, SIM_WIDTH - 4, HEIGHT - 4), 3)
        
        # Rysuj ciała
        for body in self.bodies:
            body.draw(self.screen)
        
        # Rysuj panel kontrolny
        pygame.draw.rect(self.screen, (30, 30, 30), (SIM_WIDTH, 0, PANEL_WIDTH, HEIGHT))
        
        # Rysuj nagłówki ciał z lepszym odstępem
        headers = ["Ciało 1 (Czerwone):", "Ciało 2 (Zielone):", "Ciało 3 (Niebieskie):"]
        header_y_positions = [35, 255, 475]  # Dokładne pozycje Y dla każdego nagłówka
        
        for i, (header, y_pos) in enumerate(zip(headers, header_y_positions)):
            header_surface = self.small_font.render(header, True, COLORS[i])
            self.screen.blit(header_surface, (SIM_WIDTH + 20, y_pos - 20))
        
        # Rysuj input boxy
        for box in self.input_boxes:
            box.draw(self.screen)
        
        # Rysuj przyciski
        self.start_button.draw(self.screen)
        self.pause_button.draw(self.screen)
        self.reset_button.draw(self.screen)
        
        # Wyświetl siły działające na ciała (prawy panel)
        if len(self.bodies) > 0 and self.simulating:
            force_y = 550
            force_x = 10
            force_title = self.small_font.render("Siły działające:", True, WHITE)
            self.screen.blit(force_title, (force_x, force_y))
            
            body_names = ["Czerwone", "Zielone", "Niebieskie", "Pomarańczowe", "Żółte"]
            for i, body in enumerate(self.bodies[:3]):  # Maksymalnie 3 ciała
                y_offset = force_y + 20 + i * 65
                
                # Nazwa ciała
                name_text = f"{body_names[i]}:"
                name_surface = self.small_font.render(name_text, True, body.color)
                self.screen.blit(name_surface, (force_x, y_offset))
                
                # Oblicz kąt kierunku siły (0-359 stopni)
                angle_rad = math.atan2(body.current_force_y, body.current_force_x)
                angle_deg = (math.degrees(angle_rad) + 360) % 360
                
                # Siła wypadkowa Fw
                fw_text = f"Fw={body.current_force:.2e} N"
                fw_surface = self.small_font.render(fw_text, True, WHITE)
                self.screen.blit(fw_surface, (force_x, y_offset + 15))
                
                # Kierunek
                dir_text = f"Kierunek: {angle_deg:.0f}°"
                dir_surface = self.small_font.render(dir_text, True, WHITE)
                self.screen.blit(dir_surface, (force_x, y_offset + 30))
                
                # Składowe Fx i Fy
                fx_text = f"Fx={body.current_force_x:.2e} N"
                fy_text = f"Fy={body.current_force_y:.2e} N"
                fx_surface = self.small_font.render(fx_text, True, WHITE)
                fy_surface = self.small_font.render(fy_text, True, WHITE)
                self.screen.blit(fx_surface, (force_x, y_offset + 45))
                self.screen.blit(fy_surface, (force_x + 130, y_offset + 45))
        
        # Rysuj checkboxy dla opcji
        # Checkbox łączenia ciał
        checkbox_color = DARK_GRAY if self.simulating else WHITE
        pygame.draw.rect(self.screen, checkbox_color, self.merge_checkbox_rect, 2)
        if self.merge_enabled:
            pygame.draw.line(self.screen, GREEN, 
                           (self.merge_checkbox_rect.x + 3, self.merge_checkbox_rect.y + 10),
                           (self.merge_checkbox_rect.x + 8, self.merge_checkbox_rect.y + 15), 3)
            pygame.draw.line(self.screen, GREEN,
                           (self.merge_checkbox_rect.x + 8, self.merge_checkbox_rect.y + 15),
                           (self.merge_checkbox_rect.x + 17, self.merge_checkbox_rect.y + 5), 3)
        
        merge_label = self.small_font.render("Łączenie przy kolizji", True, checkbox_color)
        self.screen.blit(merge_label, (self.merge_checkbox_rect.x + 30, self.merge_checkbox_rect.y))
        
        # Checkbox ścian
        pygame.draw.rect(self.screen, checkbox_color, self.walls_checkbox_rect, 2)
        if self.walls_enabled:
            pygame.draw.line(self.screen, GREEN,
                           (self.walls_checkbox_rect.x + 3, self.walls_checkbox_rect.y + 10),
                           (self.walls_checkbox_rect.x + 8, self.walls_checkbox_rect.y + 15), 3)
            pygame.draw.line(self.screen, GREEN,
                           (self.walls_checkbox_rect.x + 8, self.walls_checkbox_rect.y + 15),
                           (self.walls_checkbox_rect.x + 17, self.walls_checkbox_rect.y + 5), 3)
        
        walls_label = self.small_font.render("Odbijanie od ścian", True, checkbox_color)
        self.screen.blit(walls_label, (self.walls_checkbox_rect.x + 30, self.walls_checkbox_rect.y))
        
        # Rysuj slider prędkości
        speed_label = self.small_font.render("Prędkość symulacji:", True, WHITE)
        self.screen.blit(speed_label, (SIM_WIDTH + 20, 720))
        
        pygame.draw.rect(self.screen, GRAY, self.speed_slider_rect)
        pygame.draw.rect(self.screen, WHITE, self.speed_slider_handle)
        
        speed_text = self.small_font.render(f"{self.speed:.1f}x", True, WHITE)
        self.screen.blit(speed_text, (SIM_WIDTH + 240, 698))
        
        # Informacje o symulacji (lewy górny róg)
        if len(self.bodies) > 0:
            info_y = 10
            info_surface = self.small_font.render(f"Liczba ciał: {len(self.bodies)}", True, WHITE)
            self.screen.blit(info_surface, (10, info_y))
            
            # Info o sterowaniu
            if self.simulating:
                status_text = "Symulacja działa"
                status_color = GREEN
            else:
                status_text = "PAUZA"
                status_color = YELLOW
            status_surface = self.small_font.render(status_text, True, status_color)
            self.screen.blit(status_surface, (10, info_y + 25))
        
        # Prędkości w lewym dolnym rogu
        if len(self.bodies) > 0 and self.simulating:
            vel_start_y = HEIGHT - 210
            vel_title = self.small_font.render("Prędkości:", True, WHITE)
            self.screen.blit(vel_title, (10, vel_start_y))
            
            body_names = ["Czerwone", "Zielone", "Niebieskie", "Pomarańczowe", "Żółte"]
            
            for i, body in enumerate(self.bodies[:3]):  # Maksymalnie 3 ciała
                y_offset = vel_start_y + 20 + i * 60
                
                # Nazwa ciała
                name_surface = self.small_font.render(f"{body_names[i]}:", True, body.color)
                self.screen.blit(name_surface, (10, y_offset))
                
                # Prędkość wypadkowa
                velocity = math.sqrt(body.vx**2 + body.vy**2)
                vel_value = f"V={velocity:.1f} m/s"
                vel_value_surface = self.small_font.render(vel_value, True, WHITE)
                self.screen.blit(vel_value_surface, (10, y_offset + 15))
                
                # Składowe prędkości
                vx_text = f"Vx={body.vx:.1f} m/s"
                vy_text = f"Vy={body.vy:.1f} m/s"
                vx_surface = self.small_font.render(vx_text, True, WHITE)
                vy_surface = self.small_font.render(vy_text, True, WHITE)
                self.screen.blit(vx_surface, (10, y_offset + 30))
                self.screen.blit(vy_surface, (10, y_offset + 43))
        
        pygame.display.flip()
    
    def run(self):
        while self.running:
            self.handle_events()
            self.update_simulation()
            self.draw()
            self.clock.tick(60)
        
        pygame.quit()
        sys.exit()


if __name__ == "__main__":
    simulator = Simulator()
    simulator.run()
