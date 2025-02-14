uniffi::include_scaffolding!("snapxrust");

use xcap::{image::{imageops, EncodableLayout, RgbaImage}, Monitor, Window};


fn normalized(filename: &str) -> String {
    filename
        .replace("|", "")
        .replace("\\", "")
        .replace(":", "")
        .replace("/", "")
}
pub struct ImageData {
    image: Vec<u8>,
    width: u32,
    height: u32,
}
pub struct MonitorData {
    width: u32,
    height: u32,
    x: i32,
    y: i32,
    name: String
}
pub fn get_screen_dimensions(name: &str) -> MonitorData {
    let monitors = Monitor::all().unwrap();

    let monitor = monitors
        .iter()
        .find(|m| m.name() == name)
        .unwrap(); // Will panic if no monitor is found, as the name is assumed valid
    MonitorData {
        width: monitor.width(),
        height: monitor.height(),
        x: monitor.x(),
        y: monitor.y(),
        name: monitor.name().to_string()
    }
}
pub fn get_working_area() -> MonitorData {
    let monitors = Monitor::all().unwrap();

    let width: u32 = monitors.iter().map(|m| m.width()).sum();

    let height: u32 = monitors.iter().map(|m| m.height()).max().unwrap_or(0);
    let name = monitors.iter()
    .map(|m| m.name().to_string())
    .collect::<Vec<String>>()
    .join(", ");


    MonitorData {
        width,
        height,
        x: 0,
        y: 0,
        name,
    }
}

pub fn get_monitor(x: u32, y: u32) -> MonitorData {
    let monitor = Monitor::from_point(x as i32, y as i32).unwrap();
    MonitorData {
        width: monitor.width(),
        height: monitor.height(),
        x: x as i32,
        y: y as i32,
        name: monitor.name().to_string()
    }
}
pub fn get_primary_monitor() -> MonitorData {
    let monitors = Monitor::all().unwrap();

    let monitor = monitors
        .iter()
        .find(|m| m.is_primary())
        .unwrap();
    MonitorData {
        width: monitor.width(),
        height: monitor.height(),
        x: monitor.x(),
        y: monitor.y(),
        name: monitor.name().to_string()
    }
}

pub fn capture_monitor(name: &str) -> ImageData {
    let monitors = Monitor::all().unwrap();

    let monitor = monitors
        .iter()
        .find(|m| m.name() == name)
        .unwrap();

    let image = monitor.capture_image().unwrap();
    let image_bytes = image.as_bytes().to_vec();
    let width = image.width();
    let height = image.height();

    ImageData {
        image: image_bytes,
        width,
        height,
    }
}
pub fn capture_fullscreen() -> ImageData {
    let monitors = Monitor::all().unwrap();

    let total_width = monitors.iter()
        .map(|m| (m.x() as u32 + m.width()))
        .max()
        .unwrap();

    let total_height = monitors.iter()
        .map(|m| (m.y() as u32 + m.height()))
        .max()
        .unwrap();


    let mut combined_image = RgbaImage::new(total_width, total_height);

    for monitor in monitors {
        let monitor_image = monitor.capture_image().unwrap();

        let monitor_x = monitor.x() as u32;
        let monitor_y = monitor.y() as u32;
        let monitor_width = monitor_image.width();
        let monitor_height = monitor_image.height();

        for y in 0..monitor_height {
            for x in 0..monitor_width {
                let pixel = monitor_image.get_pixel(x, y);
                combined_image.put_pixel(x + monitor_x, y + monitor_y, *pixel); // Place the pixel at the correct position
            }
        }
    }

    let width = combined_image.width();
    let height = combined_image.height();
    let image_bytes = combined_image.as_bytes().to_vec();

    ImageData {
        image: image_bytes,
        width,
        height,
    }
}
pub fn capture_window(x: u32, y: u32) -> ImageData {
    let windows = Window::all().unwrap();

    let mut sorted_windows: Vec<_> = windows.iter().collect();
    sorted_windows.sort_by(|a, b| b.z().cmp(&a.z()));
    println!("Total windows: {}", sorted_windows.len());
    for (index, w) in sorted_windows.iter().enumerate() {
        println!(
            "[{}] {} {} {} {}",
            index,
            w.title(),
            w.app_name(),
            w.pid(),
            w.z()
        );
    }

    let window = sorted_windows
        .iter()
        .find(|w| {
            let win_x = w.x() as u32;
            let win_y = w.y() as u32;
            let win_width = w.width();
            let win_height = w.height();

            x >= win_x && x <= win_x + win_width && y >= win_y && y <= win_y + win_height
        })
        .unwrap();

    let image = window.capture_image().unwrap();
    let image_bytes = image.as_bytes().to_vec();
    let width = image.width();
    let height = image.height();

    ImageData {
        image: image_bytes,
        width,
        height,
    }
}




pub fn capture_rect(x: u32, y: u32, width: u32, height: u32) -> ImageData {
    let monitor = Monitor::from_point(x as i32, y as i32).unwrap();
    let mut full_image = monitor.capture_image().unwrap();
    let image = imageops::crop(&mut full_image, x, y, width, height).to_image();


    let width = image.width();
    let height = image.height();
    let image_bytes = image.as_bytes().to_vec();

    ImageData {
        image: image_bytes,
        width,
        height,
    }
}
